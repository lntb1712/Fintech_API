using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.RequestModel
{
    public class SysDeviceRequest
    {
        public Guid UserId { get; set; }
        public string? UDID { get; set; }
        public bool IsActive { get; set; } = false;
       
    }

    public class SysDeviceRequestValidator : AbstractValidator<SysDeviceRequest>
    {
        public SysDeviceRequestValidator()
        {
           
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId không được để trống.");

          
       
        }
    }
}
