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
   git clone https://github.com/Ser1-1y/BarotraumaConversationsTranslate.git
   cd BarotraumaConversationsTranslate
   ```

2. Open the project.

3. Restore the NuGet packages if necessary.

### Configuration

You can configure application using `config.json` or settings menu.

### Usage

1. Run the application.
2. Input the path to the XML file you wish to translate.
3. Translate lines. Type `Exit` to stop.
4. The translated XML will be saved, and a summary of the translation session will be displayed.

### Example XML Structure

The XML file should contain conversation nodes structured as follows:

```xml
  <Conversation line="I've been seeing an increase in rookie captains. High schoolers, applying for submarining early in life." speaker="0" maxintensity="0.4">
    <Conversation line="They hold tryouts sometimes. I've heard the standards are extremely low." speaker="1">
      <Conversation line="You've got that right." speaker="0" />
      <Conversation line="I wouldn't want any of them to captain a submarine I'm on..." speaker="2" speakertags="fearful" />
    </Conversation>
    <Conversation line="Enthusiasm hastens their demise." speaker="1" speakertags="nihilist">
      <Conversation line="I doubt that's true!" speaker="2" speakertags="optimist" />
      <Conversation line="I can't say anything jokey. This just makes me sad." speaker="2" speakertags="joker" />
    </Conversation>
  </Conversation>
```

## Contributing

Please feel free to submit a pull request or open an issue for any enhancements or bug fixes.

## License

This project is licensed under the [Apache 2.0 License](LICENSE.txt).
