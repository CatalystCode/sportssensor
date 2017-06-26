# IoT Sports Sensors Help Aspiring Amateurs Up Their Game

Recently, Microsoft partnered with the [Professional Ski Instructors of America and the American Association of Snowboard Instructors, (PSIA-AASI)](http://www.thesnowpros.org/) to use wearable IoT sensors in order to develop a machine learning model of skiing skills and expertise levels.  PSIAA-AASI continually evaluates new technology and methods for nurturing learning experiences for developing skiers and snowboarders.

In this Github repository, we are open sourcing both hardware and software that we developed in this study with the general public. This repository has two parts:

1. [How to set up the sensor kit for your sports](./SensorKit/README.md). Follow the instructions and use the scripts provided in this directory, you should be able to easily set up your hardware (sensors) and system for collecting the data from the sports. 

[NOTE] We demonstrated using some certain types of IoT sports sensors to collect data in this study. However, the same code should work on sensors from other manufacturers, as long as you are calibrating them correctly such that they all have the same sampling time stamps, and they are all referring to the same __point zero__ for their position readings (or other readings as long as __point zero__ makes sense). 

2. [How to analyze the sensory data and build machine learning models](./RScripts/README.md). The R scripts provided in this directory allow you to analyze the sensory data you collect during the sports. They can help you get insights from the sensory data to understand the major factors that differentiate the athletes with different skill levels of the sports you are investigating. Such insights can provide you some customized and quantitative guidance, no matter whether you are the athlete, or the trainer, on what the areas are for you to improve your skills. The R scripts also train a logistic regression model to categorize athletes into groups of different skill levels.

Feel free to try it out on the sports you are interested in. We also encourage you to contribute to this repository based on your experience of applying it. You can ask questions in [Issues](./issues), or even make a pull request to contribute codes or documents to this repository. We would like to work with you to make the sports we care about more aspiring and more data driven. 



