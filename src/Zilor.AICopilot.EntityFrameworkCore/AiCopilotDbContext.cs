using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Zilor.AICopilot.EntityFrameworkCore;

public class AiCopilotDbContext(DbContextOptions<AiCopilotDbContext> options) : IdentityDbContext(options);