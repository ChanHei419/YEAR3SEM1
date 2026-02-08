package com.example.lab4map

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material.icons.filled.Person
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

// ============== Register ViewModel ==============
class RegisterViewModel : ViewModel() {
    var registerResponse by mutableStateOf("Enter details to register")
    var isLoading by mutableStateOf(false)
    var registerSuccess by mutableStateOf(false)

    fun register(email: String, password: String, confirmPassword: String) {
        // Validation
        if (email.isBlank() || password.isBlank() || confirmPassword.isBlank()) {
            registerResponse = "Please fill all fields"
            return
        }

        if (!email.contains("@")) {
            registerResponse = "Please enter a valid email"
            return
        }

        if (password.length < 6) {
            registerResponse = "Password must be at least 6 characters"
            return
        }

        if (password != confirmPassword) {
            registerResponse = "Passwords do not match"
            return
        }

        isLoading = true
        viewModelScope.launch {
            try {
                val request = LoginRequest(email, password)
                val response = RetrofitInstance.api.postRegister(request)

                registerResponse = response.message
                registerSuccess = (response.status == "OK")
                isLoading = false

            } catch (e: Exception) {
                registerResponse = "Error: ${e.message}"
                isLoading = false
            }
        }
    }
}

// ============== Register Activity ==============
class RegisterActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            Lab4mapTheme {
                Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                    RegisterScreen()
                }
            }
        }
    }
}

@Composable
fun RegisterScreen() {
    val context = LocalContext.current
    val viewModel = remember { RegisterViewModel() }

    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var confirmPassword by remember { mutableStateOf("") }

    // Navigation after successful registration
    if (viewModel.registerSuccess) {
        LaunchedEffect(Unit) {
            val intent = Intent(context, LoginActivity::class.java)
            intent.putExtra("registerEmail", email)
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
        // Back Button
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp),
            contentAlignment = Alignment.TopStart
        ) {
            Button(
                onClick = {
                    val intent = Intent(context, LoginActivity::class.java)
                    context.startActivity(intent)
                },
                modifier = Modifier.width(80.dp)
            ) {
                Text("← Back", fontSize = 12.sp)
            }
        }

        // Title
        Text(
            text = "Create Account",
            fontSize = 32.sp,
            fontWeight = FontWeight.Bold,
            color = Color(0xFF2080B2),
            modifier = Modifier.padding(bottom = 32.dp)
        )

        // Subtitle
        Text(
            text = "Join Campus Navigator",
            fontSize = 16.sp,
            color = Color.Gray,
            modifier = Modifier.padding(bottom = 24.dp)
        )

        // Email Field
        OutlinedTextField(
            value = email,
            onValueChange = { email = it },
            label = { Text("Email Address") },
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
                .padding(bottom = 16.dp),
            singleLine = true
        )

        // Password Helper Text
        Text(
            text = "At least 6 characters",
            fontSize = 12.sp,
            color = Color.Gray,
            modifier = Modifier
                .align(Alignment.Start)
                .padding(start = 12.dp, bottom = 8.dp)
        )

        // Confirm Password Field
        OutlinedTextField(
            value = confirmPassword,
            onValueChange = { confirmPassword = it },
            label = { Text("Confirm Password") },
            leadingIcon = { Icon(Icons.Default.Lock, contentDescription = "Confirm Password") },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 24.dp),
            singleLine = true
        )

        // Register Button
        Button(
            onClick = { viewModel.register(email, password, confirmPassword) },
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
                Text("Create Account", fontSize = 16.sp, fontWeight = FontWeight.Bold)
            }
        }

        // Response Message
        Spacer(modifier = Modifier.height(16.dp))
        Text(
            text = viewModel.registerResponse,
            fontSize = 14.sp,
            color = if (viewModel.registerSuccess) Color.Green else Color.Red,
            modifier = Modifier.padding(horizontal = 16.dp)
        )

        // Terms
        Spacer(modifier = Modifier.height(24.dp))
        Text(
            text = "By registering, you agree to our Terms of Service",
            fontSize = 12.sp,
            color = Color.Gray,
            modifier = Modifier.padding(horizontal = 16.dp)
        )
    }
}
