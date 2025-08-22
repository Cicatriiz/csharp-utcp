// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Cli;

using System.Diagnostics;

public static class ProcessRunner
{
    public static async Task<(int exitCode, string stdOut, string stdErr)> RunAsync(CliCallTemplate template, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = template.Command,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = string.IsNullOrWhiteSpace(template.WorkingDirectory) ? Environment.CurrentDirectory : template.WorkingDirectory,
        };

        if (template.Args is not null)
        {
            foreach (var a in template.Args)
            {
                psi.ArgumentList.Add(a);
            }
        }

        if (template.Environment is not null)
        {
            foreach (var (k, v) in template.Environment)
            {
                psi.Environment[k] = v;
            }
        }

        using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var stdOut = new List<string>();
        var stdErr = new List<string>();

        p.OutputDataReceived += (_, e) => { if (e.Data is not null) stdOut.Add(e.Data); };
        p.ErrorDataReceived += (_, e) => { if (e.Data is not null) stdErr.Add(e.Data); };

        if (!p.Start())
        {
            throw new InvalidOperationException("Failed to start process");
        }

        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        var timeoutCts = template.Timeout is not null ? new CancellationTokenSource(template.Timeout.Value) : null;
        using var linked = timeoutCts is null ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        await p.WaitForExitAsync(linked.Token).ConfigureAwait(false);
        return (p.ExitCode, string.Join('\n', stdOut), string.Join('\n', stdErr));
    }
}

