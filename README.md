# eluCloud Client Template

## Description

This is a template project for creating client applications connecting to eluCloud server. It exposes `POST` and `GET` REST methods to retrieve the data from the server in a JSON format. Additionally, it utilizes SignalR to listen to the PUSH notifications from the server.

## Getting started

The *Exe* Project is a sample Console App demonstrating how to use the library.

The *eluCloud.Client* is the library that implements the communication with the server. Use the `Client` class as an entry point.

## Authors and acknowledgment

The owner of this code is elusoft GmbH. 

In case of any issues feel free to contact the author, working on behalf of elusoft GmbH. 

Mateusz Polak -
[mateusz.polak@mapo.works](mailto://mateusz.polak@mapo.works)

## License

This project is licensed under standard [MIT License](LICENSE). Feel free to fork, modify and give us a comment.

Bear in mind, that this project is using 2 external libraries that are licensed:

* **ServiceStack.Client** - [GNU Affero General Public License](https://github.com/ServiceStack/ServiceStack/blob/main/license.txt)

* **Microsoft.AspNet.SignalR.Client**, **Microsoft.AspNetCore.SignalR.Client** - [Apache 2.0 License](https://github.com/SignalR/SignalR/blob/main/LICENSE.txt)

## Project status

This project is still in development. No official release has been made so far. There still might be issues for long-running processes.
