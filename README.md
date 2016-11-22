# callstatus-notifier
This application allows to trigger an IOT device to start an indicator when skype call is in progress, and to stop it when call is stopped.

##Requirements
- .Net Framework version 4.5 or newer
- Skype (Only Skype is supported for now)

##Features
- We allow user to manually start and stop the indicator
- For Skype we
  - Notify user an easy way to start the indicator just by clicking on a popped up baloon
  - Stop the indicator when a call stops (when call is ended or rejected), provided no other call is in progress, or prompt a message stating the users who are currently active
- When the indicator is started, an LED buld is illuminated (or any other indicator preferred), and a buzzer is set to active for very short time, just to attract the people, saying a call is going to be started.

##Hardware
- Main device we are using is Particle Photon
- Indicator (LED bulb)
- Buzzer
- Transistor circuit to control the indicator via the Particle Porton
  - It might be a risk to connect the indicator directly to the Photon device, and by using a transistor, the actual current for the indicator is controlled via that transistor

##Limitations
- Currently only Skype is considered and we may extend to other communicating software as well. However, triggering manually can be used until a supported version is available.
- From the experience we got, Skype library (Skype4COM.dll) has below identified issues.
  - Don't have a way to identify the time where Skype is being updated. So, after subscribing for methods like onCallStarted, they are getting unsubscribed once Skype is restarted, after updating.
  - Sometimes, already subscribed methods are automatically unsubscribed, so that may need to put an extra effort to enhance the reliability

##License
MIT
