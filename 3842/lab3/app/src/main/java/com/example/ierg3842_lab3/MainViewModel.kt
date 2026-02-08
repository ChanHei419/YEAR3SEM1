package com.example.ierg3842_lab3

import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.launch

class MainViewModel : ViewModel() {
val message = mutableStateOf("")
val isLoggedIn = mutableStateOf(false)
val currentScreen = mutableStateOf("login")

fun navigateTo(screen: String) {
currentScreen.value = screen
message.value = ""
}

fun login(email: String, password: String) {
viewModelScope.launch {
try {
val request = LoginRequest(email, password)
val response = RetrofitInstance.api.postLogin(request)
if (response.status == "OK") {
isLoggedIn.value = true
message.value = response.message
navigateTo("logout")
} else {
message.value = response.message
}
} catch (e: Exception) {
message.value = "Error: Network request failed. ${e.message}"
}
}
}

fun register(request: RegisterRequest) {
viewModelScope.launch {
try {
val response = RetrofitInstance.api.postRegister(request)
if (response.status == "OK") {
isLoggedIn.value = true
message.value = "Registration successful! Welcome!"
navigateTo("logout")
} else {
message.value = response.message
}
} catch (e: Exception) {
message.value = "Error: Network request failed. ${e.message}"
}
}
}

fun logout() {
isLoggedIn.value = false
navigateTo("login")
message.value = "You have been logged out."
}
}