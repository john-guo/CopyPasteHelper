# CopyPasteHelper
A tool copy/paste text between pc and mobile in the same LAN 

## Build Environment
.Net Core 3.1

## Specific
Bind on the first available none loopback ipv4 device and listen in 6688 port.

## Usage

1. Startup this tool on the pc.

2. Let mobile join the LAN same as the pc.

3. Scan the qrcode with the mobile and get the url. 

4. Choose any browser to open the url.

5. Copy any text into the textarea then press "Copy to PC", the text will be copied to the pc clipboard. (Paste vice versa) 

## **Warnning**

**Due to sending the clear text without ssl, anyone in the same LAN would see the text.**

## 3rd Party Contributions

NotifyIcon WPF .Net Core Version (https://github.com/HavenDV/H.NotifyIcon.WPF)

ZXing.Net (https://github.com/micjahn/ZXing.Net/)

dat.gui (https://github.com/dataarts/dat.gui)
