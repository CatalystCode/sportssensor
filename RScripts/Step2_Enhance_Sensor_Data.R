# This script enhances the sensor data by deriving more columns to better describe the 
# activities of subjects.
# Inputs:
# It takes two inputs:
# 1. A data file with all sensor readings and 
# some columns for subject_id, experiment_id, timestamps, activity id, etc. 
# 2. A csv file describing what extra columns you want to add to better describe 
#    the subject activities. Each row should have the following columns:
#    2.1 Feature_Type: what type of feature it is. Currently support four types: Distance,
#        CenterPoint, NormalLinetoPlane, and AngleCosine
#    2.2 Output_ColName: name of the derived column added to the data
#    2.3 Input_ColName1: names of the column representing point 1
#    2.4 Input_ColName2: names of the column representing point 2
#    2.5 Input_ColName3: names of the column representing point 3
# Note: in 2.3-2.5, if multiple columns are needed to define each point, the names of these 
#    columns should be separated by :, where each column is a coordinate. 
#    For instance, for feature type NormalLinetoPlane, three points are needed to define a plane in 3-D space.
#    Each point should be defined by 3 columns: x, y, and z. Then, the names of these 3 coordinates of a point
#    such as left shoulder should be concatenated by : like leftshoulder_x:leftshoulder_y:leftshoulder_z. 
# Outputs: 
# It outputs an enhanced dataset with extra columns which are derived 
# from the original sensor data

datafile <- "C:\\Projects\\SensorReport\\final_wide_data_0422.csv"

dataset <- read.csv(datafile, sep=",", header=TRUE)

derivedSensorFile <-  "C:\\Projects\\SensorReport\\Step2_Config_DerivedSensors.csv"
dataset2 <- read.csv(derivedSensorFile, sep=",", header=TRUE, stringsAsFactors = F)
# Remove the leading and tailing spaces around Output_ColName
dataset2[,'Output_ColName'] <- gsub("^\\s+|\\s+$", "", dataset2[,'Output_ColName'])
dataset1 <- dataset[1:1000, ]

# Function to extract column names from a string, where multiple column names are 
# separated by '<sep>'
extractColNames <- function(input_cols, sep=':'){
  return(gsub("^\\s+|\\s+$", "", strsplit(input_cols, sep)[[1]]))
}

# This is the function which calculates the normal line to a plane, defined by 3 points in 3-D space. 
# To define the plane, we need to first find two directions on this plane. 
# In this function, direction 1 (d1) is from p3->p1, direction 2 (d2) is from p3->p2
# Based on right-hand rule (see https://en.wikipedia.org/wiki/Right-hand_rule for details). 
# The orthogonal direction is also a 3-D vector, and normalized such that the norm (sum of square) of this
# vector is 1.
orthogonal_direction <- function(data, input_point1, input_point2, input_point3, output_name){
  nrows <- nrow(data) 
  input_point1_array <- extractColNames(input_point1)
  input_point2_array <- extractColNames(input_point2)
  input_point3_array <- extractColNames(input_point3)
  input_point1_wrt_p3 <- data[,input_point1_array] - data[,input_point3_array]
  input_point2_wrt_p3 <- data[,input_point2_array] - data[,input_point3_array]
  
  orth_dir <- as.data.frame(array(0, dim=c(nrows,3)))
  a <- as.data.frame(array(0, dim=c(2,3)))
  for (i in 1:nrows){
    a[1,] <- input_point1_wrt_p3[i,]
    a[2,] <- input_point2_wrt_p3[i,]
    a1 <- as.matrix(a)
    orth_dir[i,1] <- det(a1[,2:3])
    orth_dir[i,2] <- det(a1[,c(1,3)])
    orth_dir[i,3] <- det(a1[,1:2])
    norm <- sqrt(sum(orth_dir[i,]^2))
    orth_dir[i,] <- orth_dir[i,]/norm
  }
  orth_dir <- data.frame(orth_dir) 
  colnames(orth_dir) <- paste(output_name, c('x','y','z'), sep='_')
  data <- data.frame(data, orth_dir)
  return(data)
}

# Function to calculate the Euclidean distance between two points. 
# These two points can be 1-D, 2-D, or 3-D. 
# These two points should be using the same dimensions. 
distance <- function(data, input_coord1, input_coord2, output_name){
  input_coord1_array <- extractColNames(input_coord1)
  input_coord2_array <- extractColNames(input_coord2)
  num_dim <- length(input_coord1_array)
  dist <- NULL
  if (num_dim == 1){
    dist <- abs(data[[input_coord1_array]] - data[[input_coord2_array]])
  } else{
    for (i in 1:num_dim){
      if (i == 1) {
        dist <- (data[[input_coord1_array[i]]] - data[[input_coord2_array[i]]])^2
      } else{
        dist <- dist + (data[[input_coord1_array[i]]] - data[[input_coord2_array[i]]])^2
      }
      
    }
    dist <- sqrt(dist)
  }
  data[[output_name]] <- dist
  return(data)
}

# This function calculates the center point between two points. 
# These two points can be 1-D, 2-D, or 3-D. 
# These two points should be using the same dimensions.
center_point <- function(data, input_coord1, input_coord2, output_name){
  input_coord1 <- extractColNames(input_coord1)
  input_coord2 <- extractColNames(input_coord2)
  data[[output_name]] <- (data[[input_coord1]] + data[[input_coord2]])/2
  return (data)
}

# This function calculates the cosine of two vectors. 
# For instance, to calculate the angle between two planes, you can calculate
# the angle between the normal directions of these two planes. 
# Function orthogonal_direction can be used to calculate the normal direction to a plane
angleCosine <- function(data, input_coord1, input_coord2, output_name){
  input_coord1_array <- extractColNames(input_coord1)
  input_coord2_array <- extractColNames(input_coord2)
  data[[output_name]] <- rowSums(data[,input_coord1_array]*data[,input_coord2_array])
  return(data)
}

# This is the main execution part of this script. 
# It calls the functions defined above to derive the extra columns.
num_extra_features <- nrow(dataset2)
for (i in 1:num_extra_features){
  feature_type_i <- dataset2[i,'Feature_Type']
  if (feature_type_i == 'Distance'){
    dataset1 <- distance(dataset1, dataset2[i, 'Input_ColName1'], dataset2[i, 'Input_ColName2'], dataset2[i, 'Output_ColName'])
  } else if (feature_type_i == 'CenterPoint'){
    dataset1 <- center_point(dataset1, dataset2[i, 'Input_ColName1'], dataset2[i, 'Input_ColName2'], dataset2[i, 'Output_ColName'])
  } else if (feature_type_i == 'NormalLinetoPlane') {
    dataset1 <- orthogonal_direction(dataset1, dataset2[i, 'Input_ColName1'], dataset2[i, 'Input_ColName2'], dataset2[i, 'Input_ColName3'], dataset2[i, 'Output_ColName'])
  } else if (feature_type_i == 'AngleCosine') {
    dataset1 <- angleCosine(dataset1, dataset2[i, 'Input_ColName1'], dataset2[i, 'Input_ColName2'], dataset2[i, 'Output_ColName'])
  }
}

# You might want to write your enhanced data to a local csv file for further analysis, 
# feature engineering and machine learning modeling. 
# You can directly use the following lines to write to a local csv file. 
# Depending on the size of the data to be written to the destination file, it may take a while.

# outputfile <- <the path and file name to a destination csv file>
# write.csv(dataset1, file=outputfile, sep=",", row.names=FALSE, quote=FALSE)
