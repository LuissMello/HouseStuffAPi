using HouseStuff.Application.Identity;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Residences;
using HouseStuff.Domain.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HouseStuff.Infrastructure.Identity;

internal static class DevelopmentDemoSeeder
{
    public static async Task SeedAsync(HouseStuffDbContext database, UserManager<HouseStuffUser> users, HouseStuffUser administrator, IConfiguration configuration)
    {
        if (!configuration.GetValue("DevelopmentDemo:Enabled", true))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var residence = administrator.ResidenceId is Guid residenceId
            ? await database.Residences.SingleAsync(item => item.Id == residenceId)
            : Residence.Create(configuration["DevelopmentDemo:ResidenceName"] ?? "Casa do Luis", administrator.Id, now).Residence!;

        if (administrator.ResidenceId is null)
        {
            database.Residences.Add(residence);
            administrator.ResidenceId = residence.Id;
            await database.SaveChangesAsync();
        }

        var memberEmail = configuration["DevelopmentDemo:MemberEmail"] ?? "luis@housestuff.local";
        var memberPassword = configuration["DevelopmentDemo:MemberPassword"] ?? "LuisHouse#2026";
        var memberName = configuration["DevelopmentDemo:MemberName"] ?? "Luis";
        var member = await users.FindByEmailAsync(memberEmail);
        if (member is null)
        {
            member = new HouseStuffUser { UserName = memberEmail, Email = memberEmail, Name = memberName, ResidenceId = residence.Id };
            var result = await users.CreateAsync(member, memberPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Não foi possível criar o morador local de demonstração.");
            }
        }
        else if (member.ResidenceId is null)
        {
            member.ResidenceId = residence.Id;
            await users.UpdateAsync(member);
        }

        if (!await users.IsInRoleAsync(member, HouseStuffRoles.Member))
        {
            await users.AddToRoleAsync(member, HouseStuffRoles.Member);
        }

        var pots = await database.Pots.Where(item => item.ResidenceId == residence.Id).ToDictionaryAsync(item => item.NormalizedName);
        var potDefinitions = new[]
        {
            (Name: "Diário", Description: "Tarefas rápidas para todos os dias.", Order: 0),
            (Name: "Semanal", Description: "Cuidados que voltam toda semana.", Order: 1),
            (Name: "Mensal", Description: "Tarefas maiores para fazer uma vez por mês.", Order: 2),
        };
        foreach (var definition in potDefinitions)
        {
            var normalized = Pot.NormalizeName(definition.Name);
            if (!pots.ContainsKey(normalized))
            {
                var pot = Pot.Create(residence.Id, definition.Name, definition.Description, definition.Order, now).Pot!;
                database.Pots.Add(pot);
                pots.Add(normalized, pot);
            }
        }
        await database.SaveChangesAsync();

        var taskDefinitions = new[]
        {
            (Pot: "Diário", Name: "Lavar a louça", Description: "Deixar a pia limpa e seca.", Kind: HouseholdTaskKind.Reusable, Days: (int?)null),
            (Pot: "Mensal", Name: "Limpar a geladeira", Description: "Revisar alimentos e higienizar as prateleiras.", Kind: HouseholdTaskKind.Recurring, Days: (int?)30),
            (Pot: "Mensal", Name: "Organizar a despensa", Description: "Separar os itens vencidos antes de reorganizar.", Kind: HouseholdTaskKind.OneTime, Days: (int?)null),
        };
        foreach (var definition in taskDefinitions)
        {
            var pot = pots[Pot.NormalizeName(definition.Pot)];
            var exists = await database.HouseholdTasks.AnyAsync(item => item.PotId == pot.Id && item.NormalizedName == HouseholdTask.NormalizeName(definition.Name));
            if (!exists)
            {
                database.HouseholdTasks.Add(HouseholdTask.Create(residence.Id, pot.Id, definition.Name, definition.Description, definition.Kind, definition.Days, now).Task!);
            }
        }
        await database.SaveChangesAsync();
    }
}
