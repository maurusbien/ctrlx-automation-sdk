# C/C++ Samples

![C++](https://upload.wikimedia.org/wikipedia/commons/thumb/1/18/ISO_C%2B%2B_Logo.svg/120px-ISO_C%2B%2B_Logo.svg.png)

## Prerequisites

* ctrlX AUTOMATION SDK build environment 1)
* ctrlX AUTOMATION SDK version 1.12 1)
* ctrlX CORE<sup>virtual</sup> or ctrlX CORE
* Visual Studio Code installed on your host computer

1) See - see Setup - Setting up a SDK QEMU VM

## Find out the supported samples 

* [Hello World](./hello.world/README.md) recommendation -> start with this example
* [Hello PLC](./hello.plc/README.md)
* [ctrlX Data Layer Client](./datalayer.client/README.md)
* [ctrlX Data Layer Client Subscription](./datalayer.client.sub/README.md)
* [ctrlX Data Layer RegisterNode](./datalayer.register.node/README.md)
* [Diagnostics Logbook](./diagnostics.logbook/README.md)
* [ctrlX Data Layer Diagnosis](./datalayer.diagnosis/README.md)
* [ctrlX Data Layer EtherCAT I/O's](./datalayer.ecat.io/README.md)
* [ctrlX Data Layer Realtime](./datalayer.realtime/README.md)
* [ctrlX Data Layer Provider All Data Types](./datalayer.provider.all-data/README.md)

## Getting Started

- Launch Visual Studio Code as your IDE
- If the IDE is started remote connect via Remote Explorer extension to your builder environment (SSH target)
- Ensure that Visual Studio Code extensions "C/C++ Extension Pack" and "CMake Tools" are installed in your SSH target.
- Open the folder of the desired cpp sample under  `samples-cpp/`

### Create an Executable for Debugging

Select the CMake symbol in the sidebar, click '...' at the top of the explorer window and select 'Clean Reconfigure All Projects'.

Then make these settings in the status bar (blue area) :

- CMake (Current build variant): Select 'Debug'
- Active kit: Select 'GCC ... x86...'
- Select 'x64...' as build target

Click Build and observe the progress in the output window - the build process should finish without errors (exit code 0).

Now the executable for debugging is available in the subdirectory: generated/__ubuntu20-gcc-x64/Debug/__

To start debugging select 'Run and Debug' in the side bar, select 'x64...' as launch configuration an press F5 (or green start button).

The executable will be started and the debugger will set automatically a breakpoint at the beginning of the main() function.

### Build a Snap

- Select the CMake symbol in the sidebar, click '...' at the top of the explorer window and select 'Clean All Projects'.
- Select main menu item Terminal --> Run Build Task --> Select the CMake Build Kit 'GCC x86...' for building a snap for a ctrlX CORE virtual or 'GCC aarch64...' for a ctrlX CORE.

At the end of the build process the snap file should be avaiable in project folder.

Right click the snap file and select 'Download'. Visual Studio Code stores it in your home directory on your host computer.

For Windows 10 this is: %USERPROFILE%

In Linux this is: ~/
                      
### Troubleshooting

All automatically created files are located in subfolders `build` and `generated`.  

If there are unclear messages during the build process, it might help to delete the folders `build` and `generated` and restart the build process.

## Support

If you've found an error in these sample, please [file an issue](https://github.com/boschrexroth)

If you've any questions visit the [ctrlX AUTOMATION Community](https://developer.community.boschrexroth.com/)

## Official Documentation

<https://docs.automation.boschrexroth.com/>