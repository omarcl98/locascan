# 🔥 Instrucciones para configurar Firebase Authentication

## Paso 1: Crear proyecto en Firebase
1. Ve a https://console.firebase.google.com/
2. Crea un nuevo proyecto llamado "apitest-demo"
3. Habilita Authentication → Email/Password

## Paso 2: Agregar aplicación Android
1. Haz clic en el ícono de Android
2. Package name: com.companyname.apitest
3. App nickname: apitest-demo
4. SHA-1: déjalo vacío por ahora
5. Registra la app

## Paso 3: Descargar configuración
1. Descarga el archivo google-services.json
2. Reemplaza el archivo en: apitest/Platforms/Android/google-services.json

## Paso 4: Crear usuario de prueba
1. Ve a Authentication → Users
2. Haz clic en "Agregar usuario"
3. Crea un usuario con email y contraseña de prueba

## Paso 5: Probar la aplicación
1. Ejecuta la aplicación
2. Usa las credenciales del usuario que creaste
3. ¡Debería funcionar la autenticación real!

## SHA-1 (opcional para demo)
Para obtener el SHA-1 real, ejecuta en Developer PowerShell:
```
keytool -list -v -keystore "%USERPROFILE%\.android\debug.keystore" -alias androiddebugkey -storepass android -keypass android
```

Pero para el demo, no es necesario.
