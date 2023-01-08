using CachingWebApi.Database;
using CachingWebApi.Models;
using CachingWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CachingWebApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DriverController : ControllerBase
{
	private readonly ICacheService _cacheService;
	private readonly CachingDbContext _cachingDbContext;
	public DriverController(ICacheService cache, CachingDbContext cachingDbContext)
	{
		_cacheService = cache;
		_cachingDbContext = cachingDbContext;
	}

	[HttpGet("Drivers")]
	public async Task<IActionResult> Get()
	{
		var cachedData = _cacheService.GetData<List<Driver>>("DRIVERS");
		if (cachedData != null && cachedData.Count() > 0)
		{
			return Ok(cachedData);
		}

		cachedData = await _cachingDbContext.Drivers.ToListAsync();

		//Set Expiry Time
		var expiryTime = DateTimeOffset.UtcNow.AddSeconds(10);
		_cacheService.SetData("DRIVERS", _cacheService, expiryTime);
		return Ok(cachedData);
	}

	[HttpPost("AddDriver")]
	public async Task<IActionResult> Post(Driver driver)
	{
		var addedObj = await _cachingDbContext.Drivers.AddAsync(driver);
		//Set Expiry Time
		var expiryTime = DateTimeOffset.UtcNow.AddSeconds(10);
		_cacheService.SetData<Driver>($"DRIVER-{driver.Id}", addedObj.Entity, expiryTime);
		await _cachingDbContext.SaveChangesAsync();
		return Ok(addedObj.Entity);
	}

	[HttpDelete("DeleteDriver")]
	public async Task<IActionResult> Delete(int id)
	{
		var exists = await _cachingDbContext.Drivers.FindAsync(id);
		if (exists == null)
			return BadRequest("Driver is not found");

		_cachingDbContext.Drivers.Remove(exists);
		_cacheService.RemoveData($"DRIVER-{id}");
		return NoContent();
	}

	[HttpPost("UpdateDriver")]
	public async Task<IActionResult> Update(Driver driver)
	{
		var exists = await _cachingDbContext.Drivers.FindAsync(driver.Id);
		if (exists == null)
			return BadRequest("Driver is not found");
		//Update the property
		exists.Name = driver.Name;
		//Save the Changes
		await _cachingDbContext.SaveChangesAsync();
		//Removed Cached Version
		_cacheService.RemoveData($"DRIVER-{driver.Id}");
		//Set Expiry Time
		var expiryTime = DateTimeOffset.UtcNow.AddSeconds(10);
		//Add Cached Verison
		_cacheService.SetData<Driver>($"DRIVER-{driver.Id}", driver, expiryTime);
		return NoContent();
	}
}
