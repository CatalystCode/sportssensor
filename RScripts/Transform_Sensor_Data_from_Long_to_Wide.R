################################################################
# This R script transforms the "long" sensory data to be "wide". 
################################################################

# Usually, the sensory data is saved in a "long" way, meaning that at each sampling point, 
# the reading of a single sensor is saved as a row in the dataset. If each sensor is measuring 10 variables, each row will have 10 columns for these 
# 10 variables. If we have 5 sensors which are sampling at the same time stamp, we will have 5 rows like this at each time stamp. 
# Such kind of data storage format makes data recording easy, but presents some challenge for data analysis and modeling. For data analysis and modeling,
# we prefer that at each time stamp, for each experimental subject, there is only one record, and each column represents a variable measured by a sensor. 
# In the previous example, if each sensor is measuring 10 variables, and we have 5 sensors, data analysis and modeling prefer to have 1 row with 50 columns
# for each sampling time stamp. We call this format of data as "Wide". 

Sys.setlocale("LC_TIME", "C")
Sys.setlocale("LC_COLLATE", "C") 
Sys.setlocale("LC_TIME", "English")

##install libraries if not installed yet
list.of.packages <- c('reshape2', 'dplyr', 'zoo', 'ggplot2', 'base64enc')
new.packages <- list.of.packages[!(list.of.packages %in% installed.packages()[,'Package'])]

if(length(new.packages))
  install.packages(new.packages)

library(base64enc)
library(dplyr)
library(reshape2) 
library(ggplot2)
library(zoo)

# Read the 1.35GB ski sensory data from Azure blob storage SAS url. Depending on the speed of internet and the configuration of your computer,
# this may take a while (even longer than a couple of hours). Alternatively, instead of 
raw_sensor_file <- "http://publicdatarepo.blob.core.windows.net/sportssensor/ski_sensor_long_anonymized.csv?sv=2014-02-14&sr=c&sig=E0%2BRNOt6%2FqiKEnVGOmV5Uu7rFYJCih9NDJYmx7wW4TU%3D&st=2017-06-22T07%3A00%3A00Z&se=2020-01-01T08%3A00%3A00Z&sp=r"
exp_dataset <- read.csv(url(raw_sensor_file), header=T, sep=",")
exp_dataset$index<-rownames(exp_dataset)

features<-c(
  "ExperimentDate",
  "ExperimentTime",
  "ExperimentTimeFraction",
  "experimentId",
  "activityTypeId",
  "subjectId",
  "tag",
  "x",
  "y",
  "z",
  "aX",
  "aY",
  "aZ",
  "qW",
  "qX",
  "qY",
  "qZ",
  "speed"
)
exp_dataset<-exp_dataset[features]

melted <- melt(exp_dataset, id.vars = c("ExperimentDate", "ExperimentTime","ExperimentTimeFraction","experimentId","activityTypeId","subjectId","tag"))
melted$variable<-paste(melted$tag,melted$variable)
melted$tag<-NULL 

widedata<- dcast(melted, ExperimentDate + ExperimentTime + ExperimentTimeFraction + experimentId + activityTypeId + subjectId  ~ variable)

