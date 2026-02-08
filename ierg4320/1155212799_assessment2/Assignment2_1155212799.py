# 1155xxxxxx
import numpy as np
import pandas as pd
from sklearn.datasets import fetch_california_housing, load_wine
import torch
import torch.nn as nn
import torch.optim as optim
from sklearn.model_selection import train_test_split, KFold
from sklearn.preprocessing import StandardScaler
from sklearn.naive_bayes import GaussianNB
from sklearn import metrics
from torchvision import datasets, transforms
from torch.utils.data import DataLoader
from sklearn.cluster import KMeans

# Problem 2
# Define the feedforward neural network regressor
class FeedForwardNN(nn.Module):
    def __init__(self, input_size):
        super().__init__()
        self.fc1 = nn.Linear(input_size, 64)
        self.fc2 = nn.Linear(64, 32)
        self.fc3 = nn.Linear(32, 1)

    def forward(self, x):
        x = torch.relu(self.fc1(x))
        x = torch.relu(self.fc2(x))
        x = self.fc3(x)
        return x

def problem_2(X, y, test_size=0.3, learning_rate=0.01, max_epochs=50000):
    testing_mse = 0
    # use this seed number for reproducibility
    # however, results from win, mac, linux are different
    # even they share the same seed number
    seed_number=5731 
    torch.manual_seed(seed_number)
    np.random.seed(seed_number)
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=test_size, random_state=seed_number)
    scaler = StandardScaler()
    
    # write your logic here: train a scaler based on training data, and apply the trained scaler on the testing data
    X_train = scaler.fit_transform(X_train)
    X_test = scaler.transform(X_test)
    
    input_size = X_train.shape[1]
    model = FeedForwardNN(input_size)
    criterion = nn.MSELoss()
    optimizer = optim.Adam(model.parameters(), lr=learning_rate)
    
    X_train_tensor = torch.FloatTensor(X_train)
    y_train_tensor = torch.FloatTensor(y_train).view(-1, 1)
    X_test_tensor = torch.FloatTensor(X_test)
    y_test_tensor = torch.FloatTensor(y_test).view(-1, 1)
    
    for epoch in range(max_epochs):
        model.train()
        # write your logic here: fill in the training loop
        optimizer.zero_grad()
        pred = model(X_train_tensor)
        loss = criterion(pred, y_train_tensor)
        loss.backward()
        optimizer.step()

    # write your logic here: evaluate the testing_mse based on testing set
    model.eval()
    with torch.no_grad():
        y_pred = model(X_test_tensor)
        mse = criterion(y_pred, y_test_tensor)
        testing_mse = mse.item()

    return testing_mse


# Problem 3
def problem_3(X, y, kfold=10):
    # write your logic here   
    model = GaussianNB()
    cv_accuracy = 0
    seed_number = 5731  # default seed is 5731
    
    kf = KFold(n_splits=kfold, shuffle=True, random_state=seed_number)
    correct_preds = 0
    
    for train_idx, test_idx in kf.split(X):
        X_train, X_test = X[train_idx], X[test_idx]
        y_train, y_test = y[train_idx], y[test_idx]
        
        fold_model = GaussianNB()
        fold_model.fit(X_train, y_train)
        y_pred = fold_model.predict(X_test)
        correct_preds+=np.sum(y_pred == y_test)
        
    cv_accuracy = correct_preds/len(y)
    
    # Re-train the model on all data
    model.fit(X, y)
    
    return cv_accuracy, model
    

# Problem 4
# write your logic here   
# define the Autoencoder according the specification
class Autoencoder(nn.Module):
    def __init__(self, input_size, hidden_size):
        super().__init__()
        self.encoder = nn.Sequential(
            nn.Linear(input_size, 512),
            nn.ReLU(),
            nn.Linear(512, 256),
            nn.ReLU(),
            nn.Linear(256, hidden_size)
        )
        self.decoder = nn.Sequential(
            nn.Linear(hidden_size, 256),
            nn.ReLU(),
            nn.Linear(256, 512),
            nn.ReLU(),
            nn.Linear(512, input_size),
            nn.Sigmoid()
        )

    def forward(self, x):
        encoded = self.encoder(x)
        decoded = self.decoder(encoded)
        return decoded, encoded

# Problem 5
def problem_5(model, loader):
    # write your logic here
    model.eval()
    latents = []
    with torch.no_grad():
        for data in loader:
            img, _ = data
            _, encoded = model(img)
            latents.append(encoded)
    
    return_array = torch.cat(latents, dim=0).cpu().numpy()
    
    return return_array


# Problem 6
def problem_6(image_descriptor, k=5):
    # write your logic here   
    kmeans = KMeans(n_clusters=k, random_state=5731, n_init='auto')
    kmeans.fit(image_descriptor)
    centres = kmeans.cluster_centers_
    
    return centres


if __name__ == "__main__":
    # Testing: Problem 2
    print("--- Testing Problem 2 ---")
    data = fetch_california_housing()
    X, y = data.data, data.target
    mse = problem_2(X, y, test_size=0.3, learning_rate=0.01, max_epochs=100)
    print("MSE in Testing: ", mse)
    print("Expected MSE in Testing: 0.3712635338306427\n")


    # Testing: Problem 3
    print("--- Testing Problem 3 ---")
    data = load_wine()
    X, y = data.data, data.target
    cv_accuracy, model = problem_3(X, y, kfold=5)
    print("5-fold cross validation accuracy:", cv_accuracy)
    print("Expected 5-fold cross validation accuracy: 0.9719101123595506")
    print("model description:", model)
    print("Expected model description: GaussianNB()\n")
   
   
    # Testing: Problem 4
    print("--- Testing Problem 4 ---")
    input_size = 28*28
    latent_size = 128 
    model = Autoencoder(input_size, latent_size)
    print("Model information:", model)
    print("\n")
    
    
    # Testing: Problem 5
    print("--- Testing Problem 5 ---")
    num_epochs = 10
    batch_size = 64
    learning_rate = 0.001
    criterion = nn.BCELoss()
    optimizer = optim.Adam(model.parameters(), lr=learning_rate)
    
    transform = transforms.Compose([
        transforms.ToTensor(),
        transforms.Lambda(lambda x: x.view(-1)),
    ])
    # Set download=False if you have already downloaded it to avoid re-downloading
    try:
        images = datasets.FashionMNIST(root='data', train=True, transform=transform, download=False)
    except RuntimeError:
        images = datasets.FashionMNIST(root='data', train=True, transform=transform, download=True)
        
    loader = DataLoader(dataset=images, batch_size=batch_size, shuffle=False)
    
    # A small training loop to make the model meaningful
    for epoch in range(num_epochs):
        for data in loader:
            img, _ = data
            output = model(img)[0]
            loss = criterion(output, img)
            optimizer.zero_grad()
            loss.backward()
            optimizer.step()
        print(f"Epoch {epoch+1}/{num_epochs} training complete.")
    
    image_descriptor = problem_5(model, loader)
    print("Image descriptor (first 3 rows):", image_descriptor[:3])
    print("Dimension:", image_descriptor.shape)
    print("Expected Dimension: (60000, 128)\n")
    
    
    # Testing: Problem 6
    print("--- Testing Problem 6 ---")
    centres = problem_6(image_descriptor, k=10)
    print("The 10 centres of the images (first 3 rows):", centres[:3])
    print("Dimension:", centres.shape)
    print("Expected Dimension: (10, 128)\n")