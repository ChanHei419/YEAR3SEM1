package com.example.lab4map


import android.Manifest
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.os.Bundle
import android.util.Log
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Chat
import androidx.compose.material.icons.filled.Person
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.core.content.ContextCompat
import com.google.android.gms.location.LocationServices
import com.google.android.gms.maps.CameraUpdateFactory
import com.google.android.gms.maps.model.BitmapDescriptor
import com.google.android.gms.maps.model.BitmapDescriptorFactory
import com.google.android.gms.maps.model.CameraPosition
import com.google.android.gms.maps.model.LatLng
import com.google.maps.android.compose.*
import com.example.lab4map.ui.theme.Lab4mapTheme
import kotlinx.coroutines.launch

// ============== Bitmap Helper ==============
fun bitmapDescriptorFromRes(
    context: Context,
    resourceId: Int,
    width: Int,
    height: Int
): BitmapDescriptor? {
    return try {
        val bitmap = BitmapFactory.decodeResource(context.resources, resourceId)
        if (bitmap != null) {
            val scaledBitmap = Bitmap.createScaledBitmap(bitmap, width, height, false)
            BitmapDescriptorFactory.fromBitmap(scaledBitmap)
        } else {
            Log.e("BitmapError", "BitmapFactory.decodeResource returned null for resource ID: $resourceId")
            null
        }
    } catch (e: Exception) {
        Log.e("BitmapError", "Error loading bitmap from resource $resourceId", e)
        null
    }
}

// ============== Data Models ==============
data class LocationData(val name: String, val latLng: LatLng, val type: String)

val defaultStartLocations = mapOf(
    0 to LocationData("Lake Ad Excellentiam (未圓湖)", LatLng(22.4139, 114.2064), "Default Start"),
    1 to LocationData("Pavilion of Harmony (合一亭)", LatLng(22.4130, 114.2087), "Default Start"),
    2 to LocationData("Chinese Pillars (華表)", LatLng(22.4187, 114.2078), "Default Start"),
    3 to LocationData("Chung Chi Gate (崇基門)", LatLng(22.4158, 114.2040), "Default Start"),
    4 to LocationData("Science Centre (科學館)", LatLng(22.4195, 114.2052), "Default Start"),
    5 to LocationData("University Library (大學圖書館)", LatLng(22.4178, 114.2071), "Default Start"),
    6 to LocationData("The Beacon (烽火台)", LatLng(22.4170, 114.2091), "Default Start"),
    7 to LocationData("Glorious United Man (聯合書院雕塑)", LatLng(22.4170, 114.2091), "Default Start"),
    8 to LocationData("Shaw Terrace (逸夫書院平台)", LatLng(22.4222, 114.2045), "Default Start"),
    9 to LocationData("Chung Chi College Chapel (崇基學院禮拜堂)", LatLng(22.4168, 114.2030), "Default Start")
)

val shopTypes = mapOf(
    0 to "7-11", 1 to "Circle K", 2 to "Fusion", 3 to "Market Place",
    4 to "Watsons", 5 to "McDonald's", 6 to "KFC",
    7 to "Pizza Hut", 8 to "MOS Burger", 9 to "Saizeriya"
)

// ============== Main Activity ==============
class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            Lab4mapTheme {
                Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                    val studentId = "1155212799"
                    val studentName = "Chan Hei Lun (1155212799)"
                    val userId = intent.getStringExtra("userId") ?: "guest"

                    Lab4MapScreen(
                        studentId = studentId,
                        studentName = studentName,
                        userId = userId
                    )
                }
            }
        }
    }
}

// ============== Main Composable ==============
@Composable
fun Lab4MapScreen(studentId: String, studentName: String, userId: String) {
    val context = LocalContext.current
    val coroutineScope = rememberCoroutineScope()

    val shopIcon = remember {
        bitmapDescriptorFromRes(context, R.drawable.shop_icon, 100, 100)
    }
    val lastDigit = studentId.last().digitToInt()
    val thirdLastDigit = studentId.getOrNull(studentId.length - 3)?.digitToInt() ?: 0

    val defaultStartLocation = defaultStartLocations[lastDigit]!!
    val assignedShopType = shopTypes[thirdLastDigit]!!

    val shopLocations = remember(assignedShopType) {
        listOf(
            LocationData("Pizza Hut (Sha Tin Plaza)", LatLng(22.3819, 114.1884), assignedShopType),
            LocationData("Pizza Hut (Mong Kok)", LatLng(22.3180, 114.1693), assignedShopType),
            LocationData("Pizza Hut (Central)", LatLng(22.2820, 114.1565), assignedShopType)
        )
    }
    val yiaLocation = LocationData(
        "YIA (Yasumoto International Academic Park)",
        LatLng(22.41629953576823, 114.21107195570168),
        "Emulator Default Location"
    )

    // State Management
    val cameraPositionState = rememberCameraPositionState {
        position = CameraPosition.fromLatLngZoom(defaultStartLocation.latLng, 17f)
    }

    var myLocation by remember { mutableStateOf<LatLng?>(null) }

    // Location Permission
    val locationPermissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestMultiplePermissions()
    ) { permissions ->
        if (permissions[Manifest.permission.ACCESS_FINE_LOCATION] == true ||
            permissions[Manifest.permission.ACCESS_COARSE_LOCATION] == true) {
            val fusedLocationClient = LocationServices.getFusedLocationProviderClient(context)
            if (ContextCompat.checkSelfPermission(context, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED) {
                fusedLocationClient.lastLocation.addOnSuccessListener { location ->
                    if (location != null) {
                        val userLatLng = LatLng(location.latitude, location.longitude)
                        myLocation = userLatLng
                        coroutineScope.launch {
                            cameraPositionState.animate(CameraUpdateFactory.newLatLngZoom(userLatLng, 17f))
                        }
                    }
                }
            }
        } else {
            Log.d("Permission", "Location permission denied.")
        }
    }

    Box(modifier = Modifier.fillMaxSize()) {
        GoogleMap(
            modifier = Modifier.matchParentSize(),
            cameraPositionState = cameraPositionState,
            uiSettings = MapUiSettings(zoomControlsEnabled = true)
        ) {
            // Default Start Location Marker
            Marker(
                state = MarkerState(position = defaultStartLocation.latLng),
                title = defaultStartLocation.name,
                snippet = "Type: ${defaultStartLocation.type}",
                icon = BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_AZURE)
            )

            // YIA Marker
            Marker(
                state = MarkerState(position = yiaLocation.latLng),
                title = yiaLocation.name,
                snippet = yiaLocation.type,
                icon = BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_VIOLET)
            )

            // User Location Marker
            myLocation?.let { loc ->
                Marker(
                    state = MarkerState(position = loc),
                    title = "My Current Location",
                    icon = BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_GREEN)
                )
            }

            // Shop Markers
            shopLocations.forEach { shop ->
                Marker(
                    state = MarkerState(position = shop.latLng),
                    title = shop.name,
                    snippet = "Type: ${shop.type}",
                    icon = shopIcon ?: BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_ORANGE)
                )
            }
        }

        // Top Left Controls
        Column(
            modifier = Modifier
                .padding(16.dp)
                .align(Alignment.TopStart)
        ) {
            Text(
                text = studentName,
                fontSize = 18.sp,
                fontWeight = FontWeight.Bold,
                color = Color.White,
                modifier = Modifier
                    .background(Color.Black.copy(alpha = 0.7f))
                    .padding(horizontal = 8.dp, vertical = 4.dp)
            )
            Spacer(modifier = Modifier.height(8.dp))

            Button(onClick = {
                locationPermissionLauncher.launch(
                    arrayOf(
                        Manifest.permission.ACCESS_FINE_LOCATION,
                        Manifest.permission.ACCESS_COARSE_LOCATION
                    )
                )
            }) {
                Text("My Location")
            }

            Spacer(modifier = Modifier.height(16.dp))

            Text(
                text = "Quick Navigation",
                fontWeight = FontWeight.Bold,
                color = Color.White,
                modifier = Modifier
                    .background(Color.Black.copy(alpha = 0.7f))
                    .padding(horizontal = 8.dp, vertical = 4.dp)
            )
            Spacer(modifier = Modifier.height(4.dp))

            Button(onClick = {
                coroutineScope.launch {
                    cameraPositionState.animate(CameraUpdateFactory.newLatLngZoom(defaultStartLocation.latLng, 17f))
                }
            }) {
                Text("Go to Default Start")
            }

            Button(onClick = {
                coroutineScope.launch {
                    cameraPositionState.animate(CameraUpdateFactory.newLatLngZoom(yiaLocation.latLng, 17f))
                }
            }) {
                Text("Go to YIA")
            }

            shopLocations.forEach { shop ->
                Button(onClick = {
                    coroutineScope.launch {
                        cameraPositionState.animate(CameraUpdateFactory.newLatLngZoom(shop.latLng, 17f))
                    }
                }) {
                    Text("Go to ${shop.name.take(20)}")
                }
            }
        }

        // Bottom Center - Shop List
        Column(
            modifier = Modifier
                .align(Alignment.BottomCenter)
                .fillMaxWidth()
                .background(Color.White.copy(alpha = 0.9f))
                .padding(8.dp)
        ) {
            Text(
                "Assigned Shop: $assignedShopType",
                fontWeight = FontWeight.Bold,
                modifier = Modifier.padding(bottom = 4.dp)
            )
            LazyColumn(modifier = Modifier.heightIn(max = 150.dp)) {
                items(shopLocations) { shop ->
                    Text(
                        text = shop.name,
                        modifier = Modifier
                            .fillMaxWidth()
                            .clickable {
                                coroutineScope.launch {
                                    cameraPositionState.animate(
                                        CameraUpdateFactory.newLatLngZoom(
                                            shop.latLng,
                                            17f
                                        )
                                    )
                                }
                            }
                            .padding(vertical = 8.dp)
                    )
                }
            }
        }

        // Bottom Right - Floating Action Buttons (Profile & Chat)
        Column(
            modifier = Modifier
                .align(Alignment.BottomEnd)
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            // Chat Button
            FloatingActionButton(
                onClick = {
                    val intent = Intent(context, ChatActivity::class.java)
                    intent.putExtra("userId", userId)
                    intent.putExtra("shopName", assignedShopType)
                    context.startActivity(intent)
                },
                containerColor = Color(0xFF4CAF50),
                modifier = Modifier.size(56.dp)
            ) {
                Icon(Icons.Default.Chat, contentDescription = "Chat", tint = Color.White)
            }

            // Profile Button
            FloatingActionButton(
                onClick = {
                    val intent = Intent(context, ProfileActivity::class.java)
                    intent.putExtra("userId", userId)
                    context.startActivity(intent)
                },
                containerColor = Color(0xFF2196F3),
                modifier = Modifier.size(56.dp)
            ) {
                Icon(Icons.Default.Person, contentDescription = "Profile", tint = Color.White)
            }
        }
    }
}
