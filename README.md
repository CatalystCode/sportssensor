# sportssensor
Further work on sports sensor data.  Specifically looking at lower fidelity data that was transmitted through bluetooth.  Becuase of missing signal, and lack of calibration between sensors, I generated a script to align the time of the feet sensors, and to clean and drop missing and repeated signal.    Net net, while cleaning is possible, the lack of calibration or time synchronization between the pair of feet sensors makes the data suspect.  Also, drop out rate due to data problems was roughtly half of the collected sampl.e  The r script are the time alignment and data cleaning routines.

Work on Illumiski and Illumiband:
1. Alignment of miscalibrated reading pairs.
2. Cleaning of dropped and repeated signal.
