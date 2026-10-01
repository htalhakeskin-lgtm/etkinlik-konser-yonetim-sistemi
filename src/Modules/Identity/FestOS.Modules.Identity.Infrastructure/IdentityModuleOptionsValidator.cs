using FestOS.Modules.Identity.Application;
using Microsoft.Extensions.Options;

namespace FestOS.Modules.Identity.Infrastructure;

/// <summary>Validates <see cref="IdentityModuleOptions"/> with generated code (configuration §4).</summary>
[OptionsValidator]
internal sealed partial class IdentityModuleOptionsValidator : IValidateOptions<IdentityModuleOptions>;
