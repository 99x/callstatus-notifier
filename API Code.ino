// -----------------------------------
// Controlling LEDs over the Internet
// -----------------------------------

// First, let's create our "shorthand" for the pins
// Same as in the Blink an LED example:
// led1 is D0, led2 is D7

int led1 = D0;
int led2 = D7;
int buzzer = D3;

String hostNames;
String commandStatus = "off";

// Last time, we only needed to declare pins in the setup function.
// This time, we are also going to register our Spark function

void setup()
{

   // Here's the pin configuration, same as last time
   pinMode(led1, OUTPUT);
   pinMode(led2, OUTPUT);
   pinMode(buzzer, OUTPUT);
   
   // We are going to declare a Spark.variable() here so that we can access the value of the photoresistor from the cloud.
   Spark.variable("hostNames", &hostNames, STRING);
   // We are also going to declare a Spark.function so that we can turn the LED on and off from the cloud.
   Spark.function("led",ledToggle);
   // This is saying that when we ask the cloud for the function "led", it will employ the function ledToggle() from this app.
   Spark.function("clearHosts",clearAllHosts);
   
   Spark.function("remUserList", remUserList);
   
   Spark.function("buzzOthers", buzzOthers);

   // For good measure, let's also make sure both LEDs are off when we start:
   digitalWrite(led1, LOW);
   digitalWrite(led2, LOW);
   digitalWrite(buzzer, LOW);

}


// Last time, we wanted to continously blink the LED on and off
// Since we're waiting for input through the cloud this time,
// we don't actually need to put anything in the loop

void loop()
{
   if(commandStatus == "on"){
       digitalWrite(led1,HIGH);
       digitalWrite(led2,HIGH);
       delay(1000);
       digitalWrite(led1,LOW);
       digitalWrite(led2,LOW);
       delay(400);
   }
}

// We're going to have a super cool function now that gets called when a matching API request is sent
// This is the ledToggle function we registered to the "led" Spark.function earlier.

int clearAllHosts(String command){
   hostNames = ""; 
   return 0;
}

int buzzOthers(String command){
    digitalWrite(buzzer, HIGH);
    delay(500);
    digitalWrite(buzzer, LOW);
    return 0;
}

int ledToggle(String command) {
    /* Spark.functions always take a string as an argument and return an integer.
    Since we can pass a string, it means that we can give the program commands on how the function should be used.
    In this case, telling the function "on" will turn the LED on and telling it "off" will turn the LED off.
    Then, the function returns a value to us to let us know what happened.
    In this case, it will return 1 for the LEDs turning on, 0 for the LEDs turning off,
    and -1 if we received a totally bogus command that didn't do anything to the LEDs.
    */
    
    String status = "";
    String hostName = "";
    
    int pos = command.indexOf(',');
    status = command.substring(0, pos);
    hostName = command.substring(pos + 1, command.length());
    
    commandStatus = status;

    if (status=="on") {
        hostNames.concat("," + hostName);
        clearHostNamesList(command);
        digitalWrite(buzzer, HIGH);
        delay(350);
        digitalWrite(buzzer, LOW);
        return 1;
    }
    else if (status=="off") {
        clearHostNamesList(command);
        if(hostNames.equalsIgnoreCase(hostName)){
            hostNames = "";
        }
        else{
            int posOfCurrentHost = hostNames.indexOf(hostName);
            if(posOfCurrentHost > 0){
                hostNames.remove(posOfCurrentHost, hostName.length()); //Remove the comma as well
            }
        }
        digitalWrite(led1,LOW);
        digitalWrite(led2,LOW);
        return 1;
    }
    else {
        return 1;
    }
}

int remUserList(String userNm){
    int posOfCurrentHost = hostNames.indexOf(userNm);
    if(posOfCurrentHost > 0){
        hostNames.remove(posOfCurrentHost, userNm.length());
    }
    clearHostNamesList(userNm);
    return 1;
}

int clearHostNamesList(String args){
    if(hostNames.startsWith(",")){
        hostNames.remove(0,1);
    }
    if(hostNames.endsWith(",")){
        hostNames.remove(hostNames.length() - 1, 1);
    }
    
    return 1;
}

