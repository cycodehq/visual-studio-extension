using System.Collections.Generic;
using Sentry;

namespace Cycode.VisualStudio.Extension.Shared.Sentry;

public static class SentryInit {
    private static string GetSentryRelease() {
        return $"{Constants.AppName}@{Vsix.Version}";
    }

    public static void Init() {
        SentrySdk.Init(options => {
            options.Dsn = Constants.SentryDsn;
            options.Debug = Constants.SentryDebug;
            options.Release = GetSentryRelease();
            options.AutoSessionTracking = Constants.SentryAutoSessionTracking;
            options.SampleRate = Constants.SentrySampleRate;
            options.SendDefaultPii = Constants.SentrySendDefaultPii;
            options.ServerName = "";

            options.DisableNetFxInstallationsIntegration();
        });
    }

    public static void SetupScope(string userId, string tenantId) {
        SentrySdk.ConfigureScope(scope => {
            scope.SetTag("tenant_id", tenantId);
            scope.User = new SentryUser {
                Id = userId,
                Other = new Dictionary<string, string> {
                    { "tenant_id", tenantId }
                }
            };
        });
    }
}