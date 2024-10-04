# packet-monitoring-system# ModbusTCP Packet Capture and Analysis

This project captures ModbusTCP packets from a specified pcap file, filters and subscribes to ModbusTCP protocol messages, and aggregates the data by function codes. The aggregated results are stored in a database and displayed on a Blazor web application for easy visualization.

## Features

- **Packet Capture**: Reads and analyzes ModbusTCP packets from pcap files.
- **Data Subscription**: Subscribes to ModbusTCP messages based on function codes.
- **Data Aggregation**: Aggregates received data by function code for statistical analysis.
- **Web Visualization**: Displays aggregated results on a Blazor web page.

## Technologies Used

- C#
- MQTTnet
- PacketDotNet
- Blazor
- SQLite

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/yourrepository.git
