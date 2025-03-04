# Connecting SDK QEMU VMs to a ctrlX CORE

## Indroduction

This document describes how SDK QEMU VMs and a ctrlX CORE can be connected.

In our examples we are asuming a Windows 10 host with px.exe as local proxy.

## Connections between Host and SDK QEMU VMs

A SDK QEMU VM is always connected to it's host computer via port forwarding. So from the hosts point of view services of a SDK VM are a reachable via the address localhost:FORWARDED_PORT

In our examples below port 10022 of the host is forwarded to port 22 (SSH) of the SDK amd64 VM and port 20022 to port 22 of SDK aarch64 VM.

From the SDK QEMU VMs perspective:

* 10.0.2.2 is the IP address of the host computer
* 10.0.2.2 is also used as gateway address *1)
* http://10.0.2.2:3128 is the URL of the proxy server px.exe running on the Windows host

*1) The QEMU instance emulates a gateway server. This makes it possible that other network participants can be reached via their IP address. The QEMU internal gateway service routes the network packages to their destinations.

## Connection Between Host and ctrlX CORE Controls

There are three ways to connect ctrlX controls:

1. ctrlX CORE<sup>virtual</sup> with Port Forwarding configured in ctrlX WORKS
2. ctrlX CORE<sup>virtual</sup> with virtual Network Adapter configured in ctrlX WORKS
3. ctrlX CORE

### ctrlX CORE<sup>virtual</sup> with Port Forwarding

Like the SDK VMs the ctrlX CORE<sup>virtual</sup> is connected via port forwarding which is configured in ctrlX WORKS. When the ctrlX CORE<sup>virtual</sup> is started port forwarding is done by an internally started QEMU instance.

![ctrlX CORE virtual with port forwarding](./images/ctrlx-sdk-port-forwarding.png)

!!! important
    To enable remote Data Layer access from a SDK VM to the ctrlX add forwarding of port __2069__ and __2070__ in ctrlX WORKS. 
    
    If port 8443 of the host is forwarded to port 443 of the ctrlX (default) add sslport=8443 to the TCP connection string. Otherwise change forwarding to 443:443.

To develop apps and test Data Layer access in the SDK VM use these URLs:

    Data Layer access as client: tcp://boschrexroth:boschrexroth@10.0.2.2:2069?sslport=8443
    Data Layer access as provider: tcp://boschrexroth:boschrexroth@10.0.2.2:2070?sslport=8443


In the picture above port 12345 is used for C++ remote debugging and its forwarding has to be configured in ctrlX WORKS too.

### ctrlX CORE<sup>virtual</sup> with Network Adapter

In this case a ctrlX CORE<sup>virtual</sup> is provided by ctrlX WORKS and its internal QEMU instance with a virtual network adapter. The  IP address of this adapter is 192.168.1.1.

To connect the Windows host with the ctrlX CORE<sup>virtual</sup> a so called TAP Windows Adapter v9 is created by ctrlX WORKS.

![ctrlX CORE virtual with network adapater](./images/ctrlx-sdk.png)


To develop apps and test Data Layer access in the SDK VM use these URLs:

    Data Layer access as client: tcp://boschrexroth:boschrexroth@192.168.1.1:2069
    Data Layer access as provider: tcp://boschrexroth:boschrexroth@192.168.1.1:2070


### ctrlX CORE

The ctrlX CORE is connected to the Windows host via its network adapter. The IP address of this adapater can be used directly in the SDK VMs e.g.:

    Data Layer access as client: tcp://boschrexroth:boschrexroth@10.52.244.123:2069
    Data Layer access as provider: tcp://boschrexroth:boschrexroth@10.52.244.123:2070


