MetabolicStat

## LICENSE GPL - 3

## DISCLAIMERS

This software is not affiliated with, sponsored by, or compensated by any health care industry company or glucose monitoring company. Neither Keto-Mojo or Nutrisense companies have contributed to this work or provided any renumeration. This software has nothing to do with them.

Furthermore:

a> There is nothing expressed or implied about this software's fitness of purpose, even the purpose of analyzing the intended statistics for which it is written.

b> This software offers no medical advice. This software offers no diet advice. This software does not provide warnings or any judgments of any data.

c> There are no guarantees expressed or implied.  This software is made available AS IS.

d> I have no conflicts of interest by or for any company where that company is actively perusing wellness programs.

e> This software is based on a very old linear regression algorithm I originally used on a TI-59 calculator in 1978. This statistical algorithm was apparently published in the 1890's.

f> None of the ‘medical’ data in this project represents private patient information protected by HIPAA.  It is all test data.

## SEE your doctor if you have concerns about your data values.



## 42?

Now that you have collected your Glucose and ketone data for a few months,
(Congradulations you now know that the answer to your question is 42 :) )
how can you make sense of it and track your progress? 
(i.e. What exactly was the question you wanted to answer anyway?)

This project is designed to help you find that answer, creating meaningful liniear regressions of 'value vs 'time', and allows the user to create detailed graphs
with excell or other graphing tools, where you can explore your data with different types of graphs, run statistical analsis, and print reports to discuss with friends.

Besides an external spread sheet or other data graphing program there are no other tools required. Linear regression statistical math was published in journals 
in the 1890's'  the software class to calculate this statistic is included in this application with documentation in that class.

This project loads csv glucose and keytone data and computes linear regressions vs time of 
GKI, Ketone, and Glucose for analisis and graphing purposes.

## Purpose

The purpose of doing this analisis is to be able to look at the data and search for releationships and patterns.
This requires user involement. This software offers no advice or medical opinions on the meaning of different values.
There are no warning messages displayed or printed.

Your glucose/meter sensors may give you readings that look concerning to you. In that event you should contact your doctor or go to urgent care.
This software just crunches numbers. It makes no judgements. It gives no advice. It contains no recipies and does not recommend a diet.

## Summary

Glucose (ketone) data set can quickly grow very large.  This is particularly true when a CGM is used to collect the data.
Varius companies who provice a meter or CGM typically provide a data service to help customers grock their data.
If you wanted to graph years worth of data there could eaisly be over 15 thousand records. This program selects data from an input data archive
file, sort's in to histogram bins as a series of linear-regressions, and prints out the results for each linear 
regression.

Example:
date|name|meanx|minx|maxx|sdx|slope|N
---:|---:|---:|---:|---:|---:|---:|---:
8/28/2020|CGM-7/1/2020-8/31/2020|85.69|52|111|0.5928|.0175|255
8/31/2020|MGL-8/31/2020-10/31/2020|80.69|40|114|0.1618|-.5091|5271
10/31/2020|MGL-10/31/2020-12/31/2020|89.91|51|161|0.1615|.16984|4936
12/29/2020|MGL-12/31/2020-3/2/2021|84.46|36|140|0.1309|-.20717|5757
3/1/2021|MGL-3/2/2021-5/1/2021|86.86|44|276|0.1588|.2588|5163
10/17/2020|MGL-5/1/2021-7/1/2021|82.73|35|159|0.185|-.01413|5876
12/8/2020|MGL-7/1/2021-8/31/2021|85.94|51|196|0.1784|.01284|5106
8/31/2021|MGL-8/31/2021-10/31/2021|85.24|51|178|0.1582|.31448|4829
9/4/2021|MGL-10/31/2021-12/31/2021|77.51|36|102|0.1639|.1219|5309
12/31/2021|MGL-12/31/2021-3/2/2022|80.73|35|104|0.186|-.04755|4878
3/2/2022|MGL-3/2/2022-5/2/2022|79.11|35|169|0.1625|-.0|4916

# Usage
