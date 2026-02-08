package com.example.mass
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.mass.ui.theme.MassTheme
import java.math.BigDecimal
import java.math.MathContext
import java.math.RoundingMode
import kotlin.math.abs
import kotlin.math.roundToLong
private fun gcd(a:Long,b:Long):Long=if(b==0L) a else gcd(b,a%b)
private fun toFraction(value:Double):String{
if(value<=0) return "0"
val maxDenom=4000
var bestNum:Long=1
var bestDenom:Long=1
var minErr=abs(value-1.0)
for(d in 2..maxDenom){
val n=(value*d).roundToLong()
if(n==0L) continue
val currentErr=abs(value-n.toDouble()/d.toDouble())
if(currentErr<minErr){
minErr=currentErr
bestNum=n
bestDenom=d.toLong()
}}
    val commonDiv=gcd(bestNum,bestDenom)
return "${bestNum/commonDiv}/${bestDenom/commonDiv}"
}
private fun fmtInput(num:Double):String{
if(num==0.0) return "0"
val bigDec=num.toBigDecimal()
val rounded=bigDec.round(MathContext(5,RoundingMode.HALF_UP))
return rounded.stripTrailingZeros().toPlainString()
}
private fun fmtOutput(num:Double):String{
if(num==0.0) return "0"
val isNeg=num<0
val absNum=abs(num)
    val fmtAbsStr=if(absNum>0&&absNum<0.01){
toFraction(absNum)
}else{
val bigDec=absNum.toBigDecimal()
val rounded=bigDec.setScale(2,RoundingMode.HALF_UP)
rounded.stripTrailingZeros().toPlainString()
}
return if(isNeg) "-$fmtAbsStr" else fmtAbsStr
}
private val conversionFactors=mapOf(
"Kilogram (kg)" to 2.20462,"Pound (lb)" to 1.0,"Gram (g)" to 0.00220462,
"Ounce (oz)" to 0.0625,"Stone (st)" to 14.0,"US Ton (Short Ton)" to 2000.0,
"Imperial Ton (Long Ton)" to 2240.0
)
fun convertMass(value:Double,from:String,to:String):Double{
val fromFactor=conversionFactors[from]?:1.0
val toFactor=conversionFactors[to]?:1.0
return (value*fromFactor)/toFactor
}
class MainActivity:ComponentActivity(){
override fun onCreate(savedInstanceState:Bundle?){
super.onCreate(savedInstanceState)
setContent{
MassTheme{
    Surface(modifier=Modifier.fillMaxSize(),color=MaterialTheme.colorScheme.background){
        MassConverterApp()
    }
}
}
}
}
@Composable
fun MassConverterApp(){
val studentName="Chan Hei Lun"
val studentId="1155212799"
val lastDigit=studentId.last().digitToInt()
val assignedUnit=when(lastDigit){
0,1->Pair("Gram (g)","g")
2,3->Pair("Ounce (oz)","oz")
4,5->Pair("Stone (st)","st")
6,7->Pair("US Ton (Short Ton)","US Ton")
8,9->Pair("Imperial Ton (Long Ton)","Imp Ton")
else->Pair("Gram (g)","g")
}
val units=listOf(Pair("Kilogram (kg)","kg"),Pair("Pound (lb)","lb"),assignedUnit)
var inputVal by remember{mutableStateOf("")}
var fromUnit by remember{mutableStateOf(units[0])}
var toUnit by remember{mutableStateOf(units[1])}
var outputTxt by remember{mutableStateOf("")}
var outputColor by remember{mutableStateOf(Color.Black)}
LaunchedEffect(inputVal,fromUnit,toUnit){
val inputNum=inputVal.toDoubleOrNull()
when{
fromUnit==toUnit->{
    outputTxt="Conversion between the same unit is not allowed."
    outputColor=Color.Red
}
inputVal.isNotEmpty()&&inputNum==null->{
    outputTxt="Enter a valid numeric value."
    outputColor=Color.Red
}
inputNum!=null->{
    val converted=convertMass(inputNum,fromUnit.first,toUnit.first)
    val formattedInput=fmtInput(inputNum)
    val formattedResult=fmtOutput(converted)
    outputTxt="$formattedInput ${fromUnit.second} = $formattedResult ${toUnit.second}"
    outputColor=Color.Black
}
else->{outputTxt=""}
}
}
Column(modifier=Modifier.fillMaxSize().padding(16.dp)){
Text(text=studentName,fontSize=20.sp)
Text(text=studentId,fontSize=20.sp)
Spacer(modifier=Modifier.height(16.dp))
Text(
text="Mass Converter",fontSize=24.sp,style=MaterialTheme.typography.headlineMedium,
modifier=Modifier.align(Alignment.CenterHorizontally)
)
Spacer(modifier=Modifier.height(16.dp))
OutlinedTextField(
value=inputVal,onValueChange={inputVal=it},label={Text("Enter Value")},
keyboardOptions=KeyboardOptions(keyboardType=KeyboardType.Number),singleLine=true,
modifier=Modifier.align(Alignment.CenterHorizontally)
)
Spacer(modifier=Modifier.height(24.dp))
UnitSelector(units=units,selectedUnit=fromUnit,onUnitSelected={fromUnit=it})
Spacer(modifier=Modifier.height(16.dp))
UnitSelector(units=units,selectedUnit=toUnit,onUnitSelected={toUnit=it})
Spacer(modifier=Modifier.weight(1f))
Text(
text=outputTxt,color=outputColor,style=MaterialTheme.typography.headlineSmall,
modifier=Modifier.padding(bottom=32.dp)
)
}
}
@Composable
fun UnitSelector(
units:List<Pair<String,String>>,
selectedUnit:Pair<String,String>,
onUnitSelected:(Pair<String,String>)->Unit
){
Column(Modifier.fillMaxWidth()){
units.forEach{unit->
Row(
    verticalAlignment=Alignment.CenterVertically,
    modifier=Modifier.fillMaxWidth().clickable{onUnitSelected(unit)}.padding(vertical=4.dp)
){
    RadioButton(selected=(selectedUnit==unit),onClick={onUnitSelected(unit)})
    Spacer(modifier=Modifier.width(8.dp))
    Text(text=unit.first)
}}}}
@Preview(showBackground=true)
@Composable
fun ShowPreview(){
MassTheme{
MassConverterApp()
}
}