using Common.Authorization;
using Common.Utils;
using Controllers;
using DomainService.Interfaces.Device;
using Entity.Entities.Account;
using FluentValidation;
using Infrastructure.Implements.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.CallRecords;
using Model.RequestModel;
using Model.RequestModel.Common;

namespace API_Template.Controllers.Device
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController(IHttpContextAccessor httpContextAccessor, IDeviceService _deviceService, IValidator<SysDeviceRequest> _validator):BaseController(httpContextAccessor)
    {
        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(string keyword = "", int pageIndex = 1, int pageSize = 50)
        {
            var result = await _deviceService.GetListDevice(currentUserId, username, keyword, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("get-detail/{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await _deviceService.GetDetail(currentUserId, username, id);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(Guid id, SysDeviceRequest req)
        {
            var validationResult = await _validator.ValidateAsync(req);
            if (!validationResult.IsValid)
                return BadRequest(Utils.CreateResponseModel(validationResult.Errors[0].ErrorMessage));
            var result = await _deviceService.Update(currentUserId, username, id, req);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")] 
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deviceService.Delete(currentUserId, username, id);
            return Ok(result);
        }
        [HttpPost("create")]     
        public async Task<IActionResult> Create (SysDeviceRequest req)
        {
            var validationResult = await _validator.ValidateAsync(req);
            if (!validationResult.IsValid)
                return BadRequest(Utils.CreateResponseModel(validationResult.Errors[0].ErrorMessage));
            
            var userDevice = GetRequestDeviceInfo(Request);
            var deviceInfo = new DeviceInfoRequest
            {
                UDID = userDevice.DeviceUUID ?? "",
                DeviceName = userDevice.DeviceName,
                OSName = userDevice.DeviceOS,
                OSVersion = userDevice.DevicePlatform
            };

            var result = await _deviceService.Create( currentUserId, username, deviceInfo, req);
            return Ok(result);
        }


            
    }
}
