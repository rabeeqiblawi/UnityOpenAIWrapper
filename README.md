# OpenAI API Wrapper for Unity

This repository contains the OpenAI API Wrapper for Unity, designed to facilitate easy integration of OpenAI's powerful language and voice models, including ChatGPT and Whisper, into Unity projects.

## Features
- **ChatGPT Integration**: Easily send and receive messages using OpenAI's ChatGPT model.
- **Voice Recognition with Whisper**: Convert speech to text using OpenAI's Whisper model.
- **Text-to-Speech**: Convert text to speech using OpenAI's voice models.

## Getting Started

### Prerequisites
- Unity 2019.4 LTS or later.
- Newtonsoft.Json for Unity package.

### Installation
1. Clone the repository or download the source code.
2. Import the package into your Unity project.
3. Ensure that the Newtonsoft.Json package is properly installed in your Unity project.

### Setup
1. Obtain an API key from OpenAI.
2. Create an `OPENAIManager` script in your project and set your API key there.

## Usage

### ChatGPT API
To use the ChatGPT API in your Unity project, follow these steps:
1. Add the `ChatGPTAPIWrapper` script to a GameObject in your scene.
2. Set the `modelName` and `temperature` parameters as desired.
3. Use the `SendMessage` or `AddMessageToConversation` methods to interact with the ChatGPT model.

### Whisper and Text-to-Speech API
To use the Whisper and Text-to-Speech APIs in your Unity project, follow these steps:
1. Add the `OpenAIVoiceAPIWrapper` script to a GameObject in your scene.
2. Use the `SendWhisperRequest` method for voice recognition.
3. Use the `SendTTSRequest` method for text-to-speech functionality.
 
