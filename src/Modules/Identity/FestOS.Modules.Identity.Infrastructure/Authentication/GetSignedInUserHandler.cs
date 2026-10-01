using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

internal sealed class GetSignedInUserHandler(IdentityDbContext context, ICurrentUser currentUser)
    : IQueryHandler<GetSignedInUserQuery, SignedInUserDetails>
{
    public async Task<SignedInUserDetails> HandleAsync(GetSignedInUserQuery query, CancellationToken cancellationToken)
    {
        var userId = UserId.From(currentUser.UserId);
        User user =
            await context.Users.AsNoTracking().SingleOrDefaultAsync(found => found.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User", userId.Value);
        return SignedInUserDetails.Of(user);
    }
}
