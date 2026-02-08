package com.example.lab4map

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Phone
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.lab4map.ui.theme.Lab4mapTheme
import kotlinx.coroutines.launch

// ============== Profile ViewModel ==============
class ProfileViewModel : ViewModel() {
    var userProfile by mutableStateOf<UserProfile?>(null)
    var isLoading by mutableStateOf(true)
    var errorMessage by mutableStateOf("")
    var updateSuccess by mutableStateOf(false)

    fun loadProfile(userId: String) {
        if (userId == "guest") {
            userProfile = UserProfile(
                email = "guest@campus.edu",
                name = "Guest User",
                phone = "",
                registrationDate = null
            )
            isLoading = false
            return
        }

        viewModelScope.launch {
            try {
                val response = RetrofitInstance.api.getProfile(userId)
                if (response.status == "OK") {
                    userProfile = response.profile
                    errorMessage = ""
                } else {
                    errorMessage = "Failed to load profile"
                }
                isLoading = false
            } catch (e: Exception) {
                errorMessage = "Error: ${e.message}"
                isLoading = false
            }
        }
    }

    fun updateProfile(userId: String, name: String, phone: String) {
        if (userId == "guest") {
            errorMessage = "Cannot update guest profile"
            return
        }

        viewModelScope.launch {
            try {
                val profile = UserProfile(
                    email = userProfile?.email ?: "",
                    name = name,
                    phone = phone,
                    registrationDate = userProfile?.registrationDate
                )
                val response = RetrofitInstance.api.updateProfile(userId, profile)

                if (response.status == "OK") {
                    updateSuccess = true
                    userProfile = profile
                    errorMessage = "Profile updated successfully!"
                } else {
                    errorMessage = response.message
                }
            } catch (e: Exception) {
                errorMessage = "Error: ${e.message}"
            }
        }
    }
}

// ============== Profile Activity ==============
class ProfileActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            Lab4mapTheme {
                Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                    ProfileScreen()
                }
            }
        }
    }
}

@Composable
fun ProfileScreen() {
    val context = LocalContext.current
    val viewModel = remember { ProfileViewModel() }
    val userId = remember { (context as? ProfileActivity)?.intent?.getStringExtra("userId") ?: "guest" }

    var editMode by remember { mutableStateOf(false) }
    var editName by remember { mutableStateOf("") }
    var editPhone by remember { mutableStateOf("") }

    // Load profile on first composition
    LaunchedEffect(Unit) {
        viewModel.loadProfile(userId)
    }

    // Update edit fields when profile loads
    LaunchedEffect(viewModel.userProfile) {
        viewModel.userProfile?.let { profile ->
            editName = profile.name ?: ""
            editPhone = profile.phone ?: ""
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        // Header
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 24.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            IconButton(onClick = {
                (context as? ProfileActivity)?.finish()
            }) {
                Icon(Icons.Default.ArrowBack, contentDescription = "Back")
            }

            Text(
                text = "User Profile",
                fontSize = 24.sp,
                fontWeight = FontWeight.Bold,
                modifier = Modifier.weight(1f),
                textAlign = androidx.compose.ui.text.style.TextAlign.Center
            )

            IconButton(onClick = {
                editMode = !editMode
                if (!editMode && viewModel.updateSuccess) {
                    viewModel.updateProfile(userId, editName, editPhone)
                }
            }) {
                Icon(Icons.Default.Edit, contentDescription = "Edit")
            }
        }

        // Loading State
        if (viewModel.isLoading) {
            Box(
                modifier = Modifier.fillMaxSize(),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator()
            }
        } else if (viewModel.userProfile != null) {
            LazyColumn(
                modifier = Modifier.fillMaxWidth()
            ) {
                // Profile Card
                item {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 16.dp),
                        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
                    ) {
                        Column(
                            modifier = Modifier.padding(16.dp)
                        ) {
                            // Email (read-only)
                            ProfileField(
                                label = "Email",
                                value = viewModel.userProfile?.email ?: "",
                                icon = Icons.Default.Email,
                                editable = false
                            )

                            Divider(modifier = Modifier.padding(vertical = 12.dp))

                            // Name (editable)
                            if (editMode) {
                                OutlinedTextField(
                                    value = editName,
                                    onValueChange = { editName = it },
                                    label = { Text("Name") },
                                    leadingIcon = { Icon(Icons.Default.Person, contentDescription = "Name") },
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(bottom = 12.dp),
                                    singleLine = true
                                )
                            } else {
                                ProfileField(
                                    label = "Name",
                                    value = viewModel.userProfile?.name ?: "Not set",
                                    icon = Icons.Default.Person,
                                    editable = false
                                )
                                Divider(modifier = Modifier.padding(vertical = 12.dp))
                            }

                            // Phone (editable)
                            if (editMode) {
                                OutlinedTextField(
                                    value = editPhone,
                                    onValueChange = { editPhone = it },
                                    label = { Text("Phone") },
                                    leadingIcon = { Icon(Icons.Default.Phone, contentDescription = "Phone") },
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(bottom = 12.dp),
                                    singleLine = true
                                )
                            } else {
                                ProfileField(
                                    label = "Phone",
                                    value = viewModel.userProfile?.phone ?: "Not set",
                                    icon = Icons.Default.Phone,
                                    editable = false
                                )
                            }

                            // Registration Date
                            if (!editMode) {
                                Divider(modifier = Modifier.padding(vertical = 12.dp))
                                Text(
                                    text = "Member since: ${viewModel.userProfile?.registrationDate ?: "Unknown"}",
                                    fontSize = 12.sp,
                                    color = Color.Gray
                                )
                            }
                        }
                    }
                }

                // Edit Mode Actions
                if (editMode) {
                    item {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(vertical = 16.dp),
                            horizontalArrangement = Arrangement.spacedBy(12.dp)
                        ) {
                            Button(
                                onClick = {
                                    editMode = false
                                    viewModel.updateProfile(userId, editName, editPhone)
                                },
                                modifier = Modifier.weight(1f)
                            ) {
                                Text("Save")
                            }

                            Button(
                                onClick = {
                                    editMode = false
                                    editName = viewModel.userProfile?.name ?: ""
                                    editPhone = viewModel.userProfile?.phone ?: ""
                                },
                                modifier = Modifier.weight(1f),
                                colors = ButtonDefaults.buttonColors(
                                    containerColor = Color.Gray
                                )
                            ) {
                                Text("Cancel")
                            }
                        }
                    }
                }

                // Error/Success Message
                if (viewModel.errorMessage.isNotEmpty()) {
                    item {
                        Surface(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(vertical = 12.dp),
                            color = if (viewModel.updateSuccess) Color(0xFFC8E6C9) else Color(0xFFFFCDD2),
                            shape = MaterialTheme.shapes.medium
                        ) {
                            Text(
                                text = viewModel.errorMessage,
                                modifier = Modifier.padding(12.dp),
                                fontSize = 14.sp,
                                color = if (viewModel.updateSuccess) Color(0xFF2E7D32) else Color(0xFFC62828)
                            )
                        }
                    }
                }

                // Action Buttons
                item {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(vertical = 24.dp),
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Button(
                            onClick = {
                                val intent = Intent(context, ChatActivity::class.java)
                                intent.putExtra("userId", userId)
                                context.startActivity(intent)
                            },
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text("Chat with Shop", fontSize = 16.sp)
                        }

                        Button(
                            onClick = {
                                val intent = Intent(context, MainActivity::class.java)
                                context.startActivity(intent)
                            },
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Text("View Map", fontSize = 16.sp)
                        }

                        Button(
                            onClick = {
                                val intent = Intent(context, LoginActivity::class.java)
                                intent.flags = Intent.FLAG_ACTIVITY_CLEAR_TOP or Intent.FLAG_ACTIVITY_NEW_TASK
                                context.startActivity(intent)
                                (context as? ProfileActivity)?.finish()
                            },
                            modifier = Modifier.fillMaxWidth(),
                            colors = ButtonDefaults.buttonColors(
                                containerColor = Color.Red
                            )
                        ) {
                            Text("Logout", fontSize = 16.sp)
                        }
                    }
                }
            }
        } else {
            Text("Failed to load profile")
        }
    }
}

@Composable
fun ProfileField(
    label: String,
    value: String,
    icon: androidx.compose.ui.graphics.vector.ImageVector,  // ← 改為正確的類型
    editable: Boolean = false
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 8.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Icon(
            imageVector = icon,
            contentDescription = label,
            modifier = Modifier
                .size(24.dp)
                .padding(end = 12.dp),
            tint = Color(0xFF2080B2)
        )
        Column {
            Text(text = label, fontSize = 12.sp, color = Color.Gray)
            Text(text = value, fontSize = 16.sp, fontWeight = FontWeight.Medium)
        }
    }
}
