# ValorVDC Family Browser

A free dockable family browser palette for Autodesk Revit. Browse, filter, and place loaded family types directly from a persistent side panel — no more hunting through the Properties palette or Type Selector.

![ValorVDC Family Browser](Resources/Logo-100x100.png)

---

## Features

- **Dockable palette** — lives alongside your view, stays open between commands
- **Preview images** — shows the type image set in Revit, or a geometry preview if none is set
- **Custom type images** — add a `Type Image` parameter to any family (including tags) and the palette uses it
- **Search** — filter types across all categories in real time
- **Configure mode** — choose exactly which family types appear in the palette
- **Per-project or per-user settings** — save your visible type selection to the current project file or to your Windows user account (carries across projects)
- **Auto update notifications** — the palette quietly checks for new releases and shows a download link when one is available

---

## Supported Revit Versions

| Revit | Framework |
|-------|-----------|
| 2024  | .NET 4.8  |
| 2025  | .NET 8    |
| 2026  | .NET 8    |
| 2027  | .NET 10   |

---

## Installation

### Option A — MSI Installer (recommended)

1. Download the latest `ValorVDC_FamilyBrowser_x.x.x.msi` from the [Releases](https://github.com/cabowker/ValorVDC_FamilyBrowser/releases/latest) page
2. Run the installer — it copies the plugin bundle to:
   ```
   C:\ProgramData\Autodesk\ApplicationPlugins\ValorVDC_FamilyBrowser.bundle\
   ```
3. Launch Revit — the **Family Browser** button appears on the **Add-Ins** tab

### Option B — Autodesk App Store

Search for **ValorVDC Family Browser** in the [Autodesk App Store](https://apps.autodesk.com). The App Store installer handles installation and future updates automatically.

### Uninstall

Go to **Windows Settings → Apps** and uninstall **ValorVDC Family Browser**, or run the original MSI again and choose **Remove**.

---

## Getting Started

1. Open a Revit project that has families loaded
2. On the **Add-Ins** tab, click **Family Browser** (in the *Family Tools* panel)
3. The palette opens — all loaded families are listed by category with preview images

---

## Using the Palette

### Browse Mode

| Control | Action |
|---------|--------|
| Search box | Filter types by name across all categories |
| Click a type card | Selects that type ready to place |
| Double-click a type card | Activates the Place Component command |
| **⟳** button | Reloads the palette from the current document |

### Configure Mode (⚙ button)

Configure mode lets you hide family types you don't want cluttering the palette.

1. Click the **⚙** icon in the header to enter configure mode
2. Use the **All** / **None** buttons to quickly select or deselect everything
3. Uncheck any family types you want to hide
4. Choose where to save the settings (see below)
5. Click **Save** to apply, or **Cancel** to discard

---

## Save Settings: Project vs My Account

At the bottom of configure mode you choose where your visibility selections are stored:

| Option | Where it's saved | When to use |
|--------|-----------------|-------------|
| **This Project** | Inside the `.rvt` file (Revit Extensible Storage) | When settings should travel with the project and be shared with your team |
| **My Account** | `%APPDATA%\ValorVDC\FamilyBrowser\settings.json` on your Windows user profile | When you want the same setup across all your projects |

> **Note:** My Account settings use `FamilyName | TypeName` keys so they work correctly across different projects — even when the same family is loaded into a different file with different internal IDs.

---

## Custom Type Images

The palette shows preview images in two ways:

1. **Built-in Type Image** — if a family type has an image set in its *Type Image* parameter (visible in the Family Types dialog), the palette uses it automatically
2. **Custom `Type Image` parameter** — for families that don't have the built-in parameter (such as tag families), you can add a shared parameter named exactly `Type Image` and assign a PNG or JPG image to it. The palette detects this and displays it

### Adding a custom image to a tag family

1. Open the tag family in the Family Editor
2. Go to **Family Types** → add a new parameter named `Type Image` (Instance or Type, Image category)
3. Assign a PNG image to the parameter
4. Load the family back into the project
5. Click **⟳** in the palette to refresh

---

## Updates

When a new version is released on GitHub, a small notice appears above the ValorVDC branding in the footer:

> ⬆ Update available: v1.0.2 → Download

Click **Download** to go directly to the release page. The notice only appears when the palette can reach GitHub — it fails silently if you are offline.

---

## License

MIT License — free for personal and commercial use. See [LICENSE](Installer/License.rtf) for full terms.

---

## About

Made by **[ValorVDC](https://valorvdc.com)** · Revit Plugins & Families

Family Browser is a free companion to [BIM Tools](https://valorvdc.com), our full Revit productivity suite. If you find it useful, check out BIM Tools for content management, sheet workflows, and more.

Issues and feature requests → [GitHub Issues](https://github.com/cabowker/ValorVDC_FamilyBrowser/issues)
