---

# XML Conversation Translator for Barotrauma

## Overview

It works for translating from any language that does not use Cyrillic into Russian.

## Features
- Loads conversation lines from an XML file.
- Skips lines that contain Russian text, with an option to display these lines.
- Saves the translated lines back to the original XML file.
- Tracks the number of translated lines and remaining English lines.

## Getting Started

### Prerequisites

- .NET SDK 8
- Newtonsoft.Json library

### Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd XMLConversationTranslator
   ```

2. Open the project.

3. Restore the NuGet packages if necessary.

### Configuration

The application requires a configuration file named `config.json`. If this file does not exist, the application will create one with default settings. You can choose to edit the configuration settings upon the first run.

### Usage

1. Run the application.
2. Input the path to the XML file you wish to translate. The file should have a `.xml` extension.
3. Follow the prompts to translate the conversation lines. Type `ExitTranslation` to stop the translation session.
4. The translated XML will be saved, and a summary of the translation session will be displayed.

### Example XML Structure

The XML file should contain conversation nodes structured as follows:

```xml
<Conversations>
    <Conversation line="Hello, how are you?" speakertags="Speaker1"/>
    <Conversation line="Привет, как дела?" speakertags="Speaker2"/>
</Conversations>
```

## Functions

- **ReadConfig**: Reads the configuration settings from a JSON file.
- **WriteConfig**: Writes the configuration settings to a JSON file.
- **Settings**: Allows the user to modify configuration settings.
- **XmlDocAnalysis**: Analyzes the XML document and retrieves conversation nodes.
- **WriteLine**: Prompts the user to input a translation for a given line.
- **EnglishCounter**: Counts the number of English lines in the XML file.
- **RussianCounter**: Counts the number of Russian lines in the XML file.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.

---

Feel free to modify any sections as needed before adding it to your GitHub repository!
