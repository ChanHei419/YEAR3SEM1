package com.example.ierg3842_lab3

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.ierg3842_lab3.ui.theme.IERG3842_lab3Theme
import java.util.regex.Pattern

class MainActivity : ComponentActivity() {
override fun onCreate(savedInstanceState: Bundle?) {
super.onCreate(savedInstanceState)
setContent {
IERG3842_lab3Theme {
Surface(
modifier = Modifier.fillMaxSize(),
color = MaterialTheme.colorScheme.background
) {
MainApp()
}
}
}
}
}

@Composable
fun MainApp(viewModel: MainViewModel = viewModel()) {
when (viewModel.currentScreen.value) {
"login" -> LoginScreen(viewModel)
"register" -> RegisterScreen(viewModel)
"logout" -> LogoutScreen(viewModel)
}
}

@Composable
fun LoginScreen(viewModel: MainViewModel) {
var email by remember { mutableStateOf("") }
var password by remember { mutableStateOf("") }
val message by viewModel.message

Column(
modifier = Modifier
.fillMaxSize()
.padding(40.dp),
horizontalAlignment = Alignment.CenterHorizontally
) {
Text(
text = "Chan Hei Lun (1155212799)",
fontSize = 20.sp,
color = Color.Gray,
modifier = Modifier
.fillMaxWidth()
.padding(bottom = 16.dp),
textAlign = TextAlign.Start
)

Text(
text = "Hello, welcome to login system",
fontSize = 25.sp,
color = MaterialTheme.colorScheme.primary,
modifier = Modifier
.fillMaxWidth()
.padding(bottom = 48.dp),
textAlign = TextAlign.Start
)

OutlinedTextField(
value = email,
onValueChange = { email = it },
label = { Text("Email") },
leadingIcon = { Icon(Icons.Default.Email, contentDescription = "Email") },
modifier = Modifier.fillMaxWidth(),
keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email)
)
Spacer(modifier = Modifier.height(16.dp))
OutlinedTextField(
value = password,
onValueChange = { password = it },
label = { Text("Password") },
leadingIcon = { Icon(Icons.Default.Lock, contentDescription = "Password") },
modifier = Modifier.fillMaxWidth(),
visualTransformation = PasswordVisualTransformation(),
keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password)
)
Spacer(modifier = Modifier.height(24.dp))
Button(
onClick = { viewModel.login(email, password) },
modifier = Modifier.fillMaxWidth()
) {
Text("Login")
}

Spacer(modifier = Modifier.height(8.dp))
Button(
onClick = { viewModel.navigateTo("register") },
modifier = Modifier.fillMaxWidth()
) {
Text("Register")
}

Spacer(modifier = Modifier.weight(1f))

if (message.isNotEmpty()) {
Text(
text = message,
color = if (message.contains("successful") || message.contains("Welcome")) Color.Green else Color.Red,
modifier = Modifier.padding(bottom = 16.dp),
textAlign = TextAlign.Center
)
}
}
}

@Composable
fun LogoutScreen(viewModel: MainViewModel) {
Column(
modifier = Modifier
.fillMaxSize()
.padding(40.dp),
horizontalAlignment = Alignment.CenterHorizontally,
verticalArrangement = Arrangement.Center
) {
Text(
text = "Welcome!",
fontSize = 32.sp,
color = MaterialTheme.colorScheme.primary
)
Text(
text = viewModel.message.value,
fontSize = 16.sp,
modifier = Modifier.padding(top = 8.dp, bottom = 32.dp)
)
Button(
onClick = { viewModel.logout() },
modifier = Modifier.fillMaxWidth()
) {
Text("Logout")
}
}
}

@Composable
fun RegisterScreen(viewModel: MainViewModel) {
var email by remember { mutableStateOf("") }
var password by remember { mutableStateOf("") }
var confirmPassword by remember { mutableStateOf("") }

var emailError by remember { mutableStateOf<String?>(null) }
var passwordError by remember { mutableStateOf<String?>(null) }
var confirmPasswordError by remember { mutableStateOf<String?>(null) }

val message by viewModel.message

val districts = listOf("New Territories", "Kowloon", "Hong Kong Island")
var district by remember { mutableStateOf(districts[0]) }

fun validate(): Boolean {
val emailPattern = Pattern.compile("[a-zA-Z0-9\\+\\.\\_\\%\\-\\+]{1,256}\\@[a-zA-Z0-9][a-zA-Z0-9\\-]{0,64}(\\.[a-zA-Z0-9][a-zA-Z0-9\\-]{0,25})+")
emailError = if (!emailPattern.matcher(email).matches()) "Invalid email format." else null
passwordError = if (password.length < 8 || password.length > 20) "Password must be 8-20 characters." else null
confirmPasswordError = if (password != confirmPassword) "Passwords do not match." else null

return emailError == null && passwordError == null && confirmPasswordError == null
}

Column(
modifier = Modifier
.fillMaxSize()
.padding(40.dp),
horizontalAlignment = Alignment.CenterHorizontally
) {
Text("Register", fontSize = 32.sp, color = MaterialTheme.colorScheme.primary, modifier = Modifier.padding(bottom = 24.dp))

OutlinedTextField(
value = email,
onValueChange = { email = it; emailError = null },
label = { Text("Email") },
isError = emailError != null,
supportingText = { if (emailError != null) Text(emailError!!) },
modifier = Modifier.fillMaxWidth()
)
OutlinedTextField(
value = password,
onValueChange = { password = it; passwordError = null },
label = { Text("Password (8-20 chars)") },
isError = passwordError != null,
supportingText = { if (passwordError != null) Text(passwordError!!) },
visualTransformation = PasswordVisualTransformation(),
modifier = Modifier.fillMaxWidth()
)
OutlinedTextField(
value = confirmPassword,
onValueChange = { confirmPassword = it; confirmPasswordError = null },
label = { Text("Confirm Password") },
isError = confirmPasswordError != null,
supportingText = { if (confirmPasswordError != null) Text(confirmPasswordError!!) },
visualTransformation = PasswordVisualTransformation(),
modifier = Modifier.fillMaxWidth()
)

Spacer(modifier = Modifier.height(16.dp))

Text("District", modifier = Modifier
.fillMaxWidth()
.padding(top = 8.dp))
Column(Modifier.fillMaxWidth()) {
districts.forEach { item ->
Row(
verticalAlignment = Alignment.CenterVertically,
modifier = Modifier
.fillMaxWidth()
.clickable { district = item }
.padding(vertical = 4.dp)
) {
RadioButton(
selected = (district == item),
onClick = { district = item }
)
Text(text = item, modifier = Modifier.padding(start = 8.dp))
}
}
}

Spacer(modifier = Modifier.height(24.dp))

Button(
onClick = {
if (validate()) {
val request = RegisterRequest(
email = email,
password = password,
district = district
)
viewModel.register(request)
}
},
modifier = Modifier.fillMaxWidth()
) {
Text("Register")
}
TextButton(
onClick = { viewModel.navigateTo("login") },
modifier = Modifier.padding(top = 8.dp)
) {
Text("Already have an account? Login")
}
if (message.isNotEmpty()) {
Text(
text = message,
color = if (message.contains("successful") || message.contains("Welcome")) Color.Green else Color.Red,
modifier = Modifier.padding(top = 16.dp),
textAlign = TextAlign.Center
)
}
}
}