package com.example.lab4map
import com.example.lab4map.RetrofitInstance

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.lab4map.ui.theme.Lab4mapTheme
import kotlinx.coroutines.launch

// ============== Data Models ==============

// ============== ViewModel ==============
class LoginViewModel : ViewModel() {
    var loginResponse by mutableStateOf("Awaiting input...")
    var isLoading by mutableStateOf(false)
    var loginSuccess by mutableStateOf(false)
    var userId by mutableStateOf("")

    fun login(email: String, password: String) {
        if (email.isBlank() || password.isBlank()) {
            loginResponse = "Please enter email and password"
            return
        }

        isLoading = true
        viewModelScope.launch {
            try {
                val request = LoginRequest(email, password)
                val response = RetrofitInstance.api.postLogin(request)

                loginResponse = response.message

                if (response.status == "OK") {
                    loginSuccess = true
                    userId = response.userId
                } else {
                    loginSuccess = false
                }
                isLoading = false
            } catch (e: Exception) {
                loginResponse = "Error: ${e.message}"
                isLoading = false
            }
        }
    }
}

// ============== UI ==============
class LoginActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            Lab4mapTheme {
                Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                    LoginScreen()
                }
            }
        }
    }
}

@Composable
fun LoginScreen() {
    val context = LocalContext.current
    val viewModel = remember { LoginViewModel() }

    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }

    // Navigation
    if (viewModel.loginSuccess) {
        LaunchedEffect(Unit) {
            val intent = Intent(context, MainActivity::class.java)
            intent.putExtra("userId", viewModel.userId)
            context.startActivity(intent)
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        // Title
        Text(
            text = "Campus Navigator",
            fontSize = 32.sp,
            fontWeight = FontWeight.Bold,
            color = Color(0xFF2080B2),
            modifier = Modifier.padding(bottom = 32.dp)
        )

        // Subtitle
        Text(
            text = "Login to Your Account",
            fontSize = 18.sp,
            color = Color.Gray,
            modifier = Modifier.padding(bottom = 24.dp)
        )

        // Email Field
        OutlinedTextField(
            value = email,
            onValueChange = { email = it },
            label = { Text("Email") },
            leadingIcon = { Icon(Icons.Default.Email, contentDescription = "Email") },
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp),
            singleLine = true
        )

        // Password Field
        OutlinedTextField(
            value = password,
            onValueChange = { password = it },
            label = { Text("Password") },
            leadingIcon = { Icon(Icons.Default.Lock, contentDescription = "Password") },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 24.dp),
            singleLine = true
        )

        // Login Button
        Button(
            onClick = { viewModel.login(email, password) },
            modifier = Modifier
                .fillMaxWidth()
                .height(50.dp),
            enabled = !viewModel.isLoading
        ) {
            if (viewModel.isLoading) {
                CircularProgressIndicator(
                    modifier = Modifier.size(24.dp),
                    color = Color.White,
                    strokeWidth = 2.dp
                )
            } else {
                Text("Login", fontSize = 16.sp, fontWeight = FontWeight.Bold)
            }
        }

        // Response Message
        Spacer(modifier = Modifier.height(16.dp))
        Text(
            text = viewModel.loginResponse,
            fontSize = 14.sp,
            color = if (viewModel.loginSuccess) Color.Green else Color.Red,
            modifier = Modifier.padding(horizontal = 16.dp)
        )

        // Register Link
        Spacer(modifier = Modifier.height(24.dp))
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.Center
        ) {
            Text("Don't have an account? ", fontSize = 14.sp)
            TextButton(
                onClick = {
                    val intent = Intent(context, RegisterActivity::class.java)
                    context.startActivity(intent)
                }
            ) {
                Text("Register here", fontSize = 14.sp, fontWeight = FontWeight.Bold)
            }
        }

        // Skip Login (for testing)
        Spacer(modifier = Modifier.height(16.dp))
        TextButton(
            onClick = {
                val intent = Intent(context, MainActivity::class.java)
                intent.putExtra("userId", "guest")
                context.startActivity(intent)
            }
        ) {
            Text("Continue as Guest", fontSize = 12.sp, color = Color.Gray)
        }
    }
}
