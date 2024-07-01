# Cycode Visual Studio Extension

The Cycode Visual Studio Extension is an extension for versions 2015, 2017, 2019, and 2022 that helps users to adopt a shift-left strategy, by enabling code scanning early in the development lifecycle, which could significantly help businesses avoid costly repairs and potential complications down the line.

## Features

Cycode Visual Studio Extension scans your code for the following:

* Exposed secrets, passwords, tokens, keys, and other credentials.
* More Cycode platform features coming soon!

## Installation

You can install the extension directly from the IDE. Go to:

**Extensions > Manage Extensions**

<img alt="Manage Extensions" src="https://raw.githubusercontent.com/cycodehq/visual-studio-extension/main/.github/images/manage_extensions.png" width="400">

In the search enter **"Cycode"** and click Install.

Alternatively, you can install the extension from the extension page: [Visual Studio 2015-2019](https://marketplace.visualstudio.com/items?itemName=cycode.cycode-14-16), [Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=cycode.cycode-17).

After installation, you can go to  **Extensions > Cycode > Open Tool Window** or **View > Other Windows > Cycode**.

_Note_: On older versions this might be on the top menu bar.

<img alt="Open Tool Window" src="https://raw.githubusercontent.com/cycodehq/visual-studio-extension/main/.github/images/open_tool_window.png" width="400">

The extension will then download the latest version of the Cycode CLI and installation will be complete.

<img alt="Extension Loading" src="https://raw.githubusercontent.com/cycodehq/visual-studio-extension/main/.github/images/extension_loading.png">

## Authentication

To authenticate the Cycode Visual Studio Extension, follow these steps:

1. Open the editor.
2. Open the extension window **Extensions > Cycode > Open Tool Window**
3. Click on the "Authenticate" button.

### Handling Detected Secrets

1. The scan displays a list of hardcoded secrets found in the application code.
2. Once the scan completes (either on-save or on-demand), you’ll then see the violation(s) presented in the "Error List" window.
3. To view the details of the violation, double click it in the list.
4. Next, choose how to address the detected violation(s).

## Configuring the Extension

To configure the extension, go to the extension settings to change the default settings:

1. In the Additional Parameters field, you can submit additional CLI parameters, such as `--verbose` mode for debugging purposes.
2. Use the API URL and APP URL fields to change the base URLs:
    1. On-premises Cycode customers should ask their admin for the relevant base URLs.
    2. For EU tenants, you'll need to adjust the API and APP URLs to include the EU tag:
        1. API URL: `https://api.eu.cycode.com`
        2. APP URL: `https://app.eu.cycode.com`
3. Use CLI PATH to set the path to the Cycode CLI executable. In cases where the CLI can't be downloaded due to your network configuration (for example, due to firewall rules), use this option.
4. Uncheck the Scan-on-Save option to prevent Cycode from scanning your code every time you save your work. Instead, use the Scan-on-Demand option.

Note: If the "Scan on Save" option is enabled in the extension settings, Cycode will scan the file in focus (including manifest files, such as package.json and dockerfile) for hardcoded secrets.

## Usage

To use the Cycode Visual Studio Extension, follow these steps:

1. Open the editor.
2. Open a project that uses the Cycode platform.
3. Open a file to scan.
    1. Press Ctrl+S to save a file → A scan will automatically be triggered.
    2. If the "Scan on Save" option is enabled in the extension settings, Cycode will scan the file in focus (including manifest files, such as package.json and dockerfile) for hardcoded secrets.
4. Also applies for auto-save.
5. Wait for the scan to complete and to display a success message.

## Support

If you encounter any issues or have any questions about the Cycode Visual Studio Extension, please reach out to the Cycode support team at support@cycode.com.

## License

The Cycode Visual Studio Extension is released under the MIT license. See the [LICENSE](https://github.com/cycodehq/visual-studio-extension/blob/main/LICENSE) file for more details.
