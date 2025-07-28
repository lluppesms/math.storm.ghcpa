# Lyle's Math Storm Web Example

Too often these days we let the computers and calculators and phones do all of the mental arithmetic for us,
and that skill of being able to estimate with mental math is a skill that is quickly fading away and needs
to be developed in order to succeed in life. Maybe this game will help you develop that skill!

## Introduction

This repository is an example of deploying a .NET 8 web app into an Azure Web App. The app itself is a very simple website that challenges you to a mental game of math..

---

This project is intended as a good example of using Infrastructure as Code (IaC) to deploy and manage the Azure resources, utilizing [Bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview) to deploy Azure resources declaratively.

The project also has fully automated CI/CD pipelines to deploy both the infrastructure and the application, so you can literally run one pipeline and have it create the Azure Resources, build the program, unit test the program, deploy the program to Azure, and run [Playwright](https://playwright.dev/dotnet/) smoke tests after it is deployed.

The pipelines and actions are all built modularly using templates, so you can snap them into new pipelines or use them in other projects.

---

Deployment Options include:

* [Deploy using Azure DevOps Pipelines](./.azdo/pipelines/readme.md)
* [Deploy using GitHub Actions](./.github/workflows-readme.md)
* [Deploy using AZD Command Line Tool](./.azure/readme.md)

---

[![Open in vscode.dev](https://img.shields.io/badge/Open%20in-vscode.dev-blue)][1]

[1]: https://github.com/lluppesms/math.storm.ghcpa/

[![azd Compatible](/Docs/images/AZD_Compatible.png)](/.azure/readme.md)

[![deploy.infra.and.website](https://github.com/lluppesms/math.storm.ghcpa/actions/workflows/4-bicep-build-deploy-app/badge.svg)](https://github.com/lluppesms/math.storm.ghcpa/actions/workflows/4-bicep-build-deploy-app.yml)

---

License: [MIT](./LICENSE)
