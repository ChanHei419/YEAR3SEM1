# 1155212799
# I used AI to study how panda works in python, like what does df.mean mean
import numpy as np
import pandas as pd
from scipy.stats import chisquare, wilcoxon, ttest_rel

# Problem 2
def problem_2(df):
    row_mean = 0
    row_sd = 0
    column_sample_mean = 0
    column_ss = 0
    # write your logic here
    row_mean = df.mean(axis=1)
    row_sd = df.std(axis=1)
    column_sample_mean = df.mean(axis=0)
    column_ss = df.sem(axis=0)
    
    return row_mean, row_sd, column_sample_mean, column_ss


# Problem 3
def problem_3(list_of_observation):
    p = 0
    chi2 = 0
    # write your logic here
    willingProbs = {
        2: 4/36, 3: 4/36, 4: 5/36, 5: 10/36, 6: 5/36, 7: 4/36, 8: 4/36
    }
    q4sum = range(2, 9)
    q4observed = {s: 0 for s in q4sum}
    for obs in list_of_observation:
        if obs in q4observed:
         q4observed[obs] += 1
    f_obs = [q4observed[s] for s in q4sum]
    totalObs = len(list_of_observation)
    f_exp = [willingProbs[s] * totalObs for s in q4sum]
    chi2, p = chisquare(f_obs=f_obs, f_exp=f_exp)

    return p, chi2


# Problem 4
def problem_4(df):
    pairt = w = 0
    # write your logic here
    sample1 = df[0]
    sample2 = df[1]
    _, pairt = ttest_rel(sample1, sample2)
    _, w = wilcoxon(sample1, sample2)

    return pairt, w


if __name__ == "__main__":
    # Testing: Problem 2
    print("q2:")
    df = pd.read_csv('problem2.csv', sep=',', header=None)
    print(problem_2(df))


    # Testing: Problem 3
    observation = [3,5,3,6,7,8,3,5,5,2,4,4,5,2,8,7,5,5,5,3]
    p, chi2 = problem_3(observation)
    print("q3:")
    print("p-value :", p)
    print("chi-square :", chi2)
    
    
    # Testing: Problem 4
    print("q4:")
    df = pd.read_csv('problem4.csv', sep=',', header=None)
    pairt, w = problem_4(df)
    print("p-value from paired sample T-test: ", pairt)
    print("p-value from wilcoxon signed-ranked test with T-statistics: ", w)

