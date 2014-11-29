Metro for Steam Skin Updater
=================

Automated updater for the Metro for Steam Skin available at http://metroforsteam.com/

## Purpose

This is a QND way to update your local skin folder with the remote version of the skin. 
This app will look at the available version number on the metroforsteam.com page and compare
it to the last known version downloaded by this app. If the version numbers are different,
the skin zip will be downloaded from the Deviant Art page and extracted to the local folder. 
Optionally, a notification will be sent via Pushbullet or Pushover. 

## Installation

1. Download the latest release from the [releases section](https://github.com/JakeH/SteamMetroUpdater/releases). 
2. Extract zip to desired location on your computer.
3. Edit the `settings.ini` file according to the [Settings Guide](#settings-guide) section.
4. Add `MetroUpdater.exe` as a scheduled task. See the [Scheduled Task](#scheduled-task) section for one way to accomplish this.

## Settings Guide

There are some settings that you must establish in the `settings.ini` file
before you can run this app.

#### App Section 

##### MetroHomeUri

This is the Uri to the home page for the skin project. This is where the version number and download link are discovered.

##### SkinFolder

The local folder where the downloaded skin will be extracted to. This value should look something like 
`c:\program files\steam\skins\metro\`.

##### Notifier

If blank, no notification will be sent upon successful update.

The valid non-blank values are: 

* `pushbullet` => Notifications will be sent via Pushbullet
* `pushover`=> Notifications will be sent via Pushover


#### Pushbullet Section
##### APIToken

If you choose to have Pushbullet notification, this must be your API token. This can be found on your 
Pushbullet page https://www.pushbullet.com/account

##### PushbulletAPIUri

Pushbullet API Uri. Should not need to be changed from the default.

#### Pushover Section
##### APIToken

If you choose to have Pushover notification, this needs to be the API token for the registered app your created 
in the Pushover.net system.

##### UserToken

If you choose to have Pushover notification, this needs to be your user API token.

##### APIUri

Psuhover API Uri. Should not need to be changed from the default.

## Scheduled Task

A very easy way to get this to run is to use Window's Task Scheduler. 

The following command (ran in a Command Prompt) will add a task which is ran once every 2 days at 8PM. 
You will need to replace the /TR parameter with the absolute path to where you extracted the app.

``` 
schtasks /Create /RU "SYSTEM" /SC DAILY /MO 2 /ST 20:00 /TN "Steam Metro Skin Updater" /TR "..\path-to-exe\MetroUpdater.exe"
```

If you wish to add this task manually, I only suggest that you have it ran under the System account to prevent 
the app window from showing on your desktop when the task executes.


## Notice

There's little error handling in this project. There is a log with minimal information in the app directory. If you have issues 
with this app, please include that file, or the relevant information, when you create a new GitHub issue.

This project is in no way associated with the skin creator nor Steam. You assume all risks by running this application.
