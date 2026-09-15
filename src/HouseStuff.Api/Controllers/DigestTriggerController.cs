using System.Security.Cryptography;
using System.Text;
using HouseStuff.Application.Notifications;
using HouseStuff.Infrastructure.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HouseStuff.Api.Controllers;

/// <summary>
/// Gatilho do resumo diário, chamado por um workflow agendado (fora do processo, já que a
/// máquina do Fly pode estar dormindo até essa chamada acordá-la) — autenticado por uma chave
/// compartilhada, não pelo login de um usuário.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/maintenance")]
public sealed class DigestTriggerController(IDailyDigestService digest, IOptions<DigestTriggerOptions> options) : ControllerBase
{
    [HttpPost("run-daily-digest")]
    public async Task<IActionResult> RunDailyDigest(CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var notified = await digest.RunAsync(cancellationToken);
        return Ok(new { residencesNotified = notified });
    }

    private bool IsAuthorized()
    {
        var expectedKey = options.Value.Key;
        if (string.IsNullOrEmpty(expectedKey) || !Request.Headers.TryGetValue("X-Digest-Key", out var provided))
        {
            return false;
        }

        var expected = Encoding.UTF8.GetBytes(expectedKey);
        var actual = Encoding.UTF8.GetBytes(provided.ToString());
        return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
