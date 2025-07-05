@echo off
echo Building ProtonDB Java Wrapper...
cd src
javac -cp . com/protondb/*.java
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo Build successful!
echo.
echo Available tests:
echo 1. Basic Client Test (TestClient)
echo 2. ProtonDB Driver Quick Test (DriverQuickTest)
echo 3. Comprehensive Driver Test (ProtonDBDriverTest)
echo.
set /p choice="Enter your choice (1-3): "

if "%choice%"=="1" (
    echo Running basic client test...
    java -cp . com.protondb.TestClient
) else if "%choice%"=="2" (
    echo Running ProtonDB Driver quick test...
    java -cp . com.protondb.DriverQuickTest
) else if "%choice%"=="3" (
    echo Running comprehensive driver test...
    java -cp . com.protondb.ProtonDBDriverTest
) else (
    echo Invalid choice. Running default test...
    java -cp . com.protondb.DriverQuickTest
)
pause
