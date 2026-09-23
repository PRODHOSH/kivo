<div align="center">
  <img src="kivo_main_Logo.png" width="300" alt="Kivo Logo">
  
  <h2>tell your computer simply</h2>
  <p>Kivo automates the boring stuff on your computer. You do the creative stuff.</p>
</div>

## What is Kivo?

Kivo is a fast local desktop tool. You speak to it and it figures out what you want to do on your computer. It runs entirely on your own machine. Your data is private and your voice recordings never leave your computer. 

## Features

* **Local Voice Processing:** Powered by Whisper.net for instant transcriptions. 
* **Smart Actions:** Kivo understands natural language intent and drives your OS directly.
* **Sleek Overlay:** A completely unobtrusive widget that only shows up when you need it.
* **Lightning Fast:** Zero network latency because we don't rely on slow cloud APIs.

## Tech Stack

| Area | Technologies |
| :--- | :--- |
| **Desktop Client** | <img src="https://cdn.simpleicons.org/csharp/239120" width="18" align="center" /> C# &nbsp;&nbsp; <img src="https://cdn.simpleicons.org/dotnet/512BD4" width="18" align="center" /> .NET WPF |
| **Landing Page** | <img src="https://cdn.simpleicons.org/nextdotjs/000000" width="18" align="center" /> Next.js &nbsp;&nbsp; <img src="https://cdn.simpleicons.org/react/61DAFB" width="18" align="center" /> React |
| **Intelligence** | <img src="https://cdn.simpleicons.org/openai/412991" width="18" align="center" /> Whisper.net &nbsp;&nbsp; <img src="https://cdn.simpleicons.org/huggingface/FFD21E" width="18" align="center" /> GGUF Local LLMs |

## Architecture

Here is a quick look at how the data flows when you speak to Kivo.

```mermaid
graph TD
    User((User Voice)) --> Widget[WPF Glass Overlay]
    
    subgraph Local Processing
        Widget --> Whisper[Whisper.net]
        Whisper -.-> |Text String| LLM[Local LLM]
        LLM -.-> |JSON Intent| Parser[Command Parser]
    end
    
    Parser --> OS[Windows Native APIs]
    
    style User fill:#ff9ec5,stroke:#333,stroke-width:2px,color:black
    style OS fill:#a2d2ff,stroke:#333,stroke-width:2px,color:black
    style Widget fill:#f7f8fa,stroke:#333
    style Whisper fill:#f7f8fa,stroke:#333
    style LLM fill:#f7f8fa,stroke:#333
    style Parser fill:#f7f8fa,stroke:#333
```

## Setup

1. Clone this repository.
2. Build the C# project in the `Kivo` folder using Visual Studio.
3. To run the landing page locally, use `npm run dev` in the `kivo-landing` folder.
