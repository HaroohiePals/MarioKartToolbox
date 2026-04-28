#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HaroohiePals.Nitro.NitroSystem.G2d;
using HaroohiePals.NitroKart.Course;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

static class MkdsGlobalMapConverter
{
    private const string EXECUTABLE = "ptexconv";
    private const string OUTPUT_BASENAME = "globalmap";

    private static readonly TimeSpan AvailabilityTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan RunTimeout = TimeSpan.FromSeconds(60);

    // Wrapped in Task.Run so Process.Start doesn't freeze the UI thread.
    public static Task<bool> IsAvailableAsync() => Task.Run(async () =>
    {
        Process? process = null;
        try
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName = EXECUTABLE,
                Arguments = "--help",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            });

            if (process is null)
                return false;

            using var cts = new CancellationTokenSource(AvailabilityTimeout);
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            process?.Dispose();
        }
    });

    public static Task<MkdsMapGraphics> RunAsync(string pngPath, string tempDir) => Task.Run(async () =>
    {
        var psi = new ProcessStartInfo
        {
            FileName = EXECUTABLE,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = tempDir,
        };
        psi.ArgumentList.Add("-bt4");
        psi.ArgumentList.Add("-onns");
        psi.ArgumentList.Add(pngPath);
        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add(OUTPUT_BASENAME);

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start ptexconv.");

        var outTask = process.StandardOutput.ReadToEndAsync();
        var errTask = process.StandardError.ReadToEndAsync();

        using var cts = new CancellationTokenSource(RunTimeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw new InvalidOperationException(
                $"ptexconv did not exit within {RunTimeout.TotalSeconds:F0}s.");
        }

        string stdOut = await outTask;
        string stdErr = await errTask;

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"ptexconv exited with code {process.ExitCode}.\n{stdErr}\n{stdOut}");

        string ncgrPath = Path.Combine(tempDir, $"{OUTPUT_BASENAME}.ncgr");
        string nclrPath = Path.Combine(tempDir, $"{OUTPUT_BASENAME}.nclr");
        string nscrPath = Path.Combine(tempDir, $"{OUTPUT_BASENAME}.nscr");

        if (!File.Exists(ncgrPath) || !File.Exists(nclrPath) || !File.Exists(nscrPath))
            throw new InvalidOperationException(
                $"ptexconv did not produce the expected files in '{tempDir}'.");

        var ncgr = new Ncgr(await File.ReadAllBytesAsync(ncgrPath));
        var nclr = new Nclr(await File.ReadAllBytesAsync(nclrPath));
        var nscr = new Nscr(await File.ReadAllBytesAsync(nscrPath));

        return new MkdsMapGraphics(ncgr, nclr, nscr);
    });

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // ignored — process may have already exited or be unkillable
        }
    }
}
