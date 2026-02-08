# 1155212799
# I ask AI that how to download the needed packages, I ask AI how to use the python code that using the library, I ask AI that why problem4 is hard to download
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from wordcloud import WordCloud
import re
import nltk
from nltk.corpus import stopwords
from nltk.tokenize import word_tokenize
from nltk.stem import PorterStemmer
import yfinance as yf
import matplotlib.patheffects as path_effects
from datetime import datetime, timedelta
from sklearn.ensemble import RandomForestClassifier
from sklearn.datasets import load_wine
from sklearn.model_selection import learning_curve, train_test_split
from sklearn.metrics import accuracy_score
from sklearn.preprocessing import StandardScaler
from collections import Counter

nltk.download('stopwords')
nltk.download('punkt_tab')

# Problem 2
def problem_2(input_file, output_file="problem2.png", title="Word Cloud for problem 2", threshold=0):
    seed_number = 5731 # use this as default seed if neccessary
    # step 1: load the text from the file
    text = ""
    with open(input_file, 'r', encoding='utf-8') as file:
        text = file.read()
    
    # write your logic here    
    # step 2: convert the text to frequency dictionary
    # convert all text to lower case, remove punctuations and numbers
    words=re.findall(r'\b[a-z]+\b', text.lower())
    word_counts=Counter(words)
    # step 3: remove all words with frequency <= threshold
    filtered_word_counts={word:count for word,count in word_counts.items()if count>threshold}
    # step 4: create a word cloud based on the frequencies
    wc=WordCloud(width=1600, height=800, 
                   background_color='black', 
                   random_state=seed_number)
    wc.generate_from_frequencies(filtered_word_counts)
    plt.figure(figsize=(20, 10))
    plt.imshow(wc,interpolation='bilinear')
    plt.axis('off')
    plt.title(title,fontsize=16)
    plt.savefig(output_file, dpi=600, bbox_inches='tight')
    # do not call plt.show()
    plt.close() # Close the figure to free up memory

# Problem 3
def problem_3(input_text, output_file="problem3.png", title="Sankey Chart for Problem 3"):
    # write your logic here  
    # step 1: convert all text to lower case, remove punctuations and numbers
    # remove stop words using nltk library
    words=re.findall(r'\b[a-z]+\b', input_text.lower())
    stop_words=set(stopwords.words('english'))
    tokens=[word for word in words if word not in stop_words]
    # step 2: convert tokens to stems
    stemmer=PorterStemmer()
    stems=[stemmer.stem(token) for token in tokens]
   # step 3: create Sankey data
    
    
    # step 4: create Sankey chart
    fig,ax=plt.subplots(figsize=(10,12))
    y_pos=np.arange(len(tokens))
    ax.set_yticks(y_pos)
    ax.set_yticklabels(tokens)
    ax.set_xlim(-0.1, 1.1)
    ax.set_xticks([]) # Hide x-axis ticks
    ax.tick_params(axis='y', length=0, pad=5) # Adjust padding
    ax.spines['top'].set_visible(False)
    ax.spines['bottom'].set_visible(False)
    ax.spines['left'].set_visible(False)
    ax.spines['right'].set_visible(False)
    ax2 = ax.twinx()
    ax2.set_ylim(ax.get_ylim())
    ax2.set_yticks(y_pos)
    ax2.set_yticklabels(stems)
    ax2.tick_params(axis='y', length=0, pad=5)
    ax2.spines['top'].set_visible(False)
    ax2.spines['bottom'].set_visible(False)
    ax2.spines['left'].set_visible(False)
    ax2.spines['right'].set_visible(False)
    for i in range(len(tokens)):
        ax.plot([0, 1], [i, i], color='c', alpha=0.6, linewidth=2)
    plt.title(title, fontsize=14, pad=20)
    
    plt.savefig(output_file, bbox_inches='tight')
    # do not call plt.show()
    plt.close()

# Problem 4
def problem_4(tickers,start_date,end_date,output_file="problem4.png",title="Stock Price Comparison for Problem 4"):
     # download data
    data = yf.download(tickers, start=start_date, end=end_date)['Close']
    # write your logic here  
    if isinstance(data,pd.Series):data=data.to_frame()
    if data is None or len(data)==0 or data.dropna(how="all").shape[0]==0:
        plt.figure(figsize=(12,6));plt.title(title);plt.text(0.5,0.5,"No data downloaded",ha="center",va="center");plt.savefig(output_file);return
    data=data.dropna(how="all")
    plt.figure(figsize=(12,6))
    ax=plt.gca()
    base_palette=["#1f77b4","#ff7f0e","#2ca02c","#d62728","#9467bd","#8c564b","#e377c2","#7f7f7f","#bcbd22","#17becf"]
    custom={"AAPL":"#d62728","MSFT":"#ff7f0e","GOOGL":"#2ca02c","AMZN":"#1f77b4"}
    for i,col in enumerate(data.columns):
        color=custom.get(col,base_palette[i%len(base_palette)])
        ax.plot(data.index,data[col],color=color,lw=2,label=col)
    ax.set_title(title,fontsize=16)
    ax.set_xlabel("Date")
    ax.set_ylabel("Price (USD)")
    ax.grid(True,alpha=0.25)
    dates=data.index.to_list()
    n=len(dates)
    if n==0:
        plt.savefig(output_file);return
    mid_idx=n//2
    max_shift=max(1,int(n*0.05))
    if len(data.columns)==1:shifts=[0]
    else:
        base=list(range(-max_shift,max_shift+1))
        step=max(1,len(base)//(len(data.columns)))
        shifts=base[::step][:len(data.columns)]
        while len(shifts)<len(data.columns):
            shifts.extend(base);shifts=shifts[:len(data.columns)]
    y_span=(data.max().max()-data.min().min()) if data.size>0 else 1.0
    for i,col in enumerate(data.columns):
        color=custom.get(col,base_palette[i%len(base_palette)])
        idx=mid_idx+shifts[i];idx=max(0,min(n-1,idx))
        x=dates[idx];y=float(data.iloc[idx][col])
        txt=ax.text(x,y,f" {col} ",color=color,fontsize=11,fontweight="bold",va="center",ha="center")
        txt.set_path_effects([path_effects.Stroke(linewidth=3,foreground="white"),path_effects.Normal()])
        offset=(1 if i%2==0 else -1)*0.01*y_span
        txt.set_y(y+offset)
    plt.tight_layout()
    plt.savefig(output_file)
    # do not call plt.show()
# Problem 5
def problem_5(data, group_names, output_file="problem5.png", titles=["Subplot Exercise for Problem 5", "Box plot", "Mean values"]):
    # write your logic here
    means=[np.mean(d) for d in data]
    fig,(ax1, ax2)=plt.subplots(1, 2, figsize=(14, 7))
    colors=['#F8766D','#FDB462','#75C46B']
    bplot = ax1.boxplot(data, tick_labels=group_names, patch_artist=True, medianprops=dict(color="orange", linewidth=1.5))
    for patch, color in zip(bplot['boxes'], colors):
        patch.set_facecolor(color)
        patch.set_alpha(0.8)
    ax1.set_title(titles[1],fontsize=14, fontweight='bold')
    ax1.set_ylabel("Values",fontsize=12)
    ax1.grid(True, linestyle='--',alpha=0.5)
    bars=ax2.barh(group_names, means, color=colors)
    for bar in bars:
        bar.set_alpha(0.8)
    ax2.set_title(titles[2],fontsize=14, fontweight='bold')
    ax2.set_xlabel("Values",fontsize=12)
    ax2.grid(True,linestyle='--',alpha=0.5)
    for index, value in enumerate(means):
        ax2.text(value + 1, index, f'{value:.1f}', va='center', ha='left', fontweight='bold')
    fig.suptitle(titles[0], fontsize=16, fontweight='bold')
    plt.tight_layout(rect=[0, 0, 1, 0.96])
    plt.savefig(output_file)
    plt.close()
    # do not call plt.show()

# Problem 6
def problem_6(X, y, output_file="problem6.png", title="Learning Curve for Problem 6"):
    seed_number = 5731
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=seed_number)
    scaler = StandardScaler()
    X_train_scaled = scaler.fit_transform(X_train)
    X_test_scaled = scaler.transform(X_test)
    # write your logic here
    train_sizes_abs=[1,10,20,50,100,113]
    model=RandomForestClassifier(random_state=seed_number)
    train_sizes,train_scores,test_scores=learning_curve(
        estimator=model,
        X=X_train_scaled,
        y=y_train,
        train_sizes=train_sizes_abs,
        cv=5,
        n_jobs=-1,
        random_state=seed_number
    )
    train_scores_mean=np.mean(train_scores, axis=1)
    train_scores_std=np.std(train_scores, axis=1)
    test_scores_mean=np.mean(test_scores, axis=1)
    test_scores_std=np.std(test_scores, axis=1)
    fig, ax=plt.subplots(figsize=(12, 8))
    ax.set_title(title, fontsize=16, fontweight='bold')
    ax.set_xlabel("Training Set Size", fontsize=12)
    ax.set_ylabel("Accuracy Score", fontsize=12)
    ax.grid(True,linestyle='--', alpha=0.6)
    ax.set_ylim(0.68, 1.05)
    ax.fill_between(train_sizes, train_scores_mean - train_scores_std,
                    train_scores_mean + train_scores_std, alpha=0.1, color="blue")
    ax.fill_between(train_sizes, test_scores_mean - test_scores_std,
                    test_scores_mean + test_scores_std, alpha=0.1, color="red")

    ax.plot(train_sizes,train_scores_mean, 'o-', color="blue", label="Training score")
    ax.plot(train_sizes,test_scores_mean, 'o-', color="red", label="Cross-validation score")
    ax.legend(loc="best")
    final_train_score = train_scores_mean[-1]
    final_cv_score = test_scores_mean[-1]
    ax.text(train_sizes[-1] + 2, final_train_score, f'Final Train: {final_train_score:.3f}', 
            color='blue', va='center', ha='left', fontweight='bold')
    ax.text(train_sizes[-1] + 2, final_cv_score, f'Final CV:{final_cv_score:.3f}', 
            color='red', va='center', ha='left', fontweight='bold')
    plt.savefig(output_file)
    plt.close()


if __name__ == "__main__":
    # Testing: Problem 2
    problem_2("problem2.txt", "problem2.png", threshold=3)


    # Testing: Problem 3
    text = ""
    with open("problem3.txt", 'r', encoding='utf-8') as file:
        text = file.read()
    problem_3(text, output_file="problem3.png")
   
   
    # Testing: Problem 4
    tickers = ["AAPL", "MSFT", "GOOGL", "AMZN"]
    start_date = "2020-01-01"
    end_date = "2025-01-01"
    problem_4(tickers, start_date, end_date)

    
    # Testing: Problem 5
    np.random.seed(5731)
    # create sample data for three groups
    data1 = np.random.normal(100, 15, 300)  # Group A
    data2 = np.random.normal(110, 12, 200)  # Group B
    data3 = np.random.normal(95, 18, 400)   # Group C
    # combine data for subplots
    data = [data1, data2, data3]
    group_names = ['Group A', 'Group B', 'Group C']
    problem_5(data, group_names)
    
    
    # Testing: Problem 6
    wine = load_wine()
    X, y = wine.data, wine.target
    problem_6(X, y)