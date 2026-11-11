# Microsoft Store package (MSIX)

The MSIX package is intended for Microsoft Store distribution. It contains the same app as the ZIP release, with Windows package identity, startup integration, and the permissions required by Store-only features. The Microsoft Store signs the package after upload; this script does not use a signing certificate.

> **Keep the reserved Store identity unchanged.** The Store listing was reserved as **NotchX - Dynamic Notch**. The app's current product and in-app name are **Notch**, but `StoreName`, `IdentityName`, and `Publisher` must continue to match the existing Partner Center reservation exactly. Change them only if the Store identity itself has been changed in Partner Center.

## Requirements

- Windows 10 version 2004 (build 19041) or later, or Windows 11.
- The .NET 8 SDK.
- PowerShell.
- The exact package identity values from Partner Center.

The script uses `makeappx.exe` and `makepri.exe` from the Windows SDK when available. If they cannot be found, it attempts to restore Microsoft's `Microsoft.Windows.SDK.BuildTools` NuGet package. The first fallback build therefore needs NuGet access.

## 1. Configure the package identity

In Partner Center, open the Notch app and go to **Product management › Product identity**. Copy the values into [`identity.json`](identity.json):

| Partner Center value | `identity.json` field | Notes |
|---|---|---|
| Reserved product name | `StoreName` | Copy the reserved name exactly, including **NotchX - Dynamic Notch** if that is still the reservation. Windows and the app itself use the short name **Notch**. |
| Package/Identity/Name | `IdentityName` | Must match the reserved package identity. |
| Package/Identity/Publisher | `Publisher` | Copy the full value, including its `CN=` prefix. |
| Package/Properties/PublisherDisplayName | `PublisherDisplayName` | Copy the publisher display name exactly. |

Placeholder identity values may allow package creation, but Partner Center will reject the package. Do not substitute the app's display name for the reserved identity fields.

## 2. Build

Run from the repository root in PowerShell:

```powershell
.\packaging\build-msix.ps1
```

By default, the script builds x64 and ARM64 packages, then bundles them for one Store upload. To build a single architecture:

```powershell
.\packaging\build-msix.ps1 -Arch x64
.\packaging\build-msix.ps1 -Arch arm64
```

The output directory can be changed with a repository-relative path:

```powershell
.\packaging\build-msix.ps1 -Out artifacts\msix
```

The script recreates its `obj\msix` work directory and the selected output directory before building. Move any package you want to keep elsewhere before running it again.

### Output files

The script creates one architecture-specific package for each requested architecture:

```text
msix-out\Notch_<version>_x64.msix
msix-out\Notch_<version>_arm64.msix
```

When both architectures are built, it also creates:

```text
msix-out\Notch_<version>.msixbundle
```

For the current project version `0.4.0`, the Store package version is `0.4.0.0`. The script reads the three-part version from `src\Notchify\Notchify.csproj` and appends the required fourth component, `0`. Keep the project version in that format for Store builds.

Each package is self-contained. The build publishes the app for the selected runtime, generates the Store image assets from `Icon\notch-1024.png`, substitutes the identity and version in the manifest, creates `resources.pri`, and packs the result with `makeappx`.

## 3. Upload and submit

In Partner Center, open the Notch app, choose **Start your submission › Packages**, and upload the `.msixbundle`. Complete the rest of the submission, including the privacy policy URL, description, screenshots, and capability declarations.

An unsigned MSIX is the expected upload artifact: the Store signs it during submission. Use the ZIP release for direct downloads; the Store package is intended for Partner Center distribution.

## Continuous integration

The GitHub Actions workflow builds the app on pushes and pull requests:

- **Notch** — self-contained x64 and ARM64 ZIP build outputs.
- **Notch-Store** — the MSIX bundle.

Pushing a `v*` tag also creates x64 and ARM64 ZIP files on a GitHub Release. The workflow definition is in [`.github/workflows/build.yml`](../.github/workflows/build.yml).

## Differences from the ZIP build

- **Start with Windows:** MSIX uses the `NotchStartup` StartupTask. The ZIP build uses the current user's Windows Run key. If the user disables the Store app in **Windows Settings › Apps › Startup**, Notch cannot turn it back on itself; enable it there.
- **Other-app notification mirroring:** this feature requires the packaged build and Windows notification access.
- **App data:** Windows keeps the packaged app's data separate from the unpackaged ZIP build and removes the package-managed data when the Store app is uninstalled.
- **Store identity:** the package identity and reserved Store product name are not the same as the current user-facing name. Keep the Partner Center identity exact; the app displays **Notch**.

## Certification notes

### `runFullTrust` justification

Use the following as a starting point for the Partner Center **Restricted capabilities** justification. Review it against the capabilities and behavior in the version being submitted:

> Notch is a desktop utility that shows a notch-style panel at the top of the screen. It needs full trust to listen for volume and Caps Lock keys to show on-screen indicators (a low-level keyboard hook; keystrokes are never stored or sent, and password fields are skipped); read other windows' titles and virtual-desktop placement for its window switcher; read media sessions and audio devices for music controls and the sound mixer; show a tray icon; and convert documents locally with Word or LibreOffice. Everything runs on the user's PC. Network requests are made only by features that need them: weather and city search (Open-Meteo), song lyrics (LRCLIB), calendar feeds the user adds, translation, web search initiated by the user, video downloads initiated by the user, and GitHub Copilot usage if the user enters a token.

### Review checklist

- Provide a **privacy policy URL**; the app can read clipboard content and keyboard state for its features. The policy must describe local processing, optional network requests, and user controls.
- Keep the listing description and screenshots consistent with the submitted build.
- Use screenshots produced from the current app version; the main README describes the screenshot helper.
- Do not imply that Notch is an official Microsoft, Apple, or other third-party product.
- Re-check the restricted-capability justification whenever capabilities or data handling change.

## Troubleshooting

| Problem | Check |
|---|---|
| Partner Center rejects the package identity | Compare all four fields in `identity.json` with the current Partner Center **Product identity** values; do not use placeholder values. |
| Version is rejected | Confirm the project has a three-part version such as `0.4.0`; the script appends `.0` for the four-part Store version. |
| `makeappx` or `makepri` is not found | Install the Windows SDK, or ensure the .NET SDK can restore `Microsoft.Windows.SDK.BuildTools` from NuGet. |
| No bundle is produced | A bundle is created only when both architectures are included. A single-architecture build produces only its `.msix`. |
| A previous package disappeared | The script clears the selected output directory at the start of a build. Copy packages you need to keep to another location first. |

See the [main README](../README.md) for user-facing installation and feature information, and the [privacy policy](../docs/PRIVACY.md) for detailed data-handling disclosures.
