using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCoreApp.Security
{
    //public class CustomEmailConfirmationTokenProvider<TUser>
    // : DataProtectorTokenProvider<TUser> where TUser : class
    //{
    //    public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
    //                                    IOptions<CustomEmailConfirmationTokenProviderOptions> options,
    //                                    ILogger<TUser> logger)
    //        : base(dataProtectionProvider, options, logger)
    //    { }
    //}

    public class CustomEmailConfirmationTokenProvider<TUser>
    : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
                                        IOptions<CustomEmailConfirmationTokenProviderOptions> options,
                                        ILogger<CustomEmailConfirmationTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        { }
    }
}
