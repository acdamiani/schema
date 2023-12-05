[![Schema](https://github.com/acdamiani/schema/assets/65556364/30a3663a-482c-42f9-9e1d-86f992cee3fe)](https://schema-ai.com)

# Schema - Create AI in Unity

[![asset store](https://img.shields.io/badge/Asset_Store-100000?style=for-the-badge&logo=unity&logoColor=white)](https://assetstore.unity.com/packages/tools/behavior-ai/schema-200876) [![license](https://img.shields.io/github/license/acdamiani/schema?style=for-the-badge)](LICENSE.md) [![latest](https://img.shields.io/github/v/release/acdamiani/schema?style=for-the-badge&label=Latest)](https://github.com/acdamiani/schema/releases/latest)

Schema is a visual scripting tool that allows you to create Behavior Trees for
video games with minimal hassle and configuration.

Features:
- 150+ Nodes and Decorators
- Simple API
- Zero runtime allocations
- Sub-millisecond execution per Agent
- Extensive documentation
- Data oriented design
- Event driven
- Intuitive editor
- Supports built in Unity serialization (no manual saving or loading needed)
- Supports custom editors out of the box
- Blackboard variables
- Blackboard properties (get and set)
- Conditional aborts
- Built in tree formatting
- 100% free and open source under the [MIT License](LICENSE.md)

## Quick Start

There are two ways to download and install Schema -- through either the [asset store](https://assetstore.unity.com/packages/tools/behavior-ai/schema-200876) or through this GitHub repository. Downloading the source through GitHub is recommended if you want the latest updates; Unity's Asset Store is generally slower to receive new bugfixes and features. Unity is designed to work for Unity versions `2020.1` and above.

### Asset Store

The Asset store page is located [here](https://assetstore.unity.com/packages/tools/behavior-ai/schema-200876). The asset is free, so all you need to do to add Schema to your library is to click the "Add to My Assets" button. In your Unity project, download and import the asset through the Package Manager. You can find instructions to do this on Unity's [tutorial website](https://learn.unity.com/tutorial/the-package-manager#5f6060d2edbc2a001ee93975). This is the recommended installation solution for most people, since it is integrated into the engine.

### GitHub

You can download the [latest](https://github.com/acdamiani/schema/releases/latest) `.unitypackage` archive file from the releases tab. If you want the latest updates, download the source code or clone the repository and move the code into your project folder.

```bash
git clone "https://github.com/acdamiani/schema" Schema
# Move to your project folder
mv Schema your/project/directory
```

## Examples

All examples can be found in the `Samples~` folder in the root of the project. The `Example` project contains a simple behavior tree with a custom node and fully commented code describing what all of the scripts are doing. Open the included scene to see the capabilities of the package.

To get started by making your own tree, checkout the `Readme!` PDF. To go in more depth to the workings of the project, check out the `Docs` folder, which contains Markdown files relating the features of Schema.

## Contact

If you run into any trouble, have suggestions, or just want to talk about the project, feel free to shoot me an email at damiani.august@gmail.com

A huge thank you to everyone who has helped with this project--your work means a lot!
