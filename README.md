
# La carrera por el titulo 

 integrantes, roles, versión exacta del motor y librerías utilizadas

integrantes: Dalmiro Martins Capitao, Paloma Ferrara, Ayelen Aranda

Motor: Unity, version 2022.0362f3

Inteligencias Artificiales utilizadas: Chatgpt y Gemini 

Diseño de juego (MDA) — marco Mecánicas, Dinámicas y Estéticas

1. Mecánicas (Mechanics)

Sistema de Movimiento Cíclico:

Lanzamiento de dado numérico (1 a 6) mediante interacción directa por botón en la UI.

Avance paso a paso del peón visual sobre un tablero 3D cíclico de 28 casilleros.

Estructura de juego en 3 vueltas completas, representando 3 años de carrera universitaria.

Sistema de Evaluación (Trivia):

Casilleros de Pregunta: Al aterrizar en una casilla especial, el flujo de juego se pausa para abrir un panel desplegable de pregunta.

Selección Múltiple: Cada evento muestra una pregunta aleatoria leída desde un archivo de datos (.json) con 4 opciones de respuesta y 1 sola opción correcta.

Temporizador Dinámico: Cuenta regresiva activa por pregunta que obliga a responder dentro de un límite de tiempo fijado.

Mecánica de Examen de Fin de Año :

Verificación de Desempeño: Al completar la vuelta del tablero, el sistema evalúa el contador acumulado de RespuestasCorrectas del jugador.

Criterio de Aprobación: El jugador requiere una cuota mínima de aciertos por vuelta (ej. 3 respuestas correctas para graduarse a la Vuelta 2).

Penalización por Reprobación: Si el jugador no alcanza la cuota de respuestas correctas al cruzar la meta, no podra avanzar hasta aprobar.

Gestión de Turnos Local:

Rotación de turnos entre hasta 4 jugadores locales en una misma pantalla/dispositivo.

2. Dinámicas (Dynamics)

Presión y Toma de Decisiones Bajo Tiempo: El temporizador en los casilleros de evaluación rompe la contemplación pasiva y genera una dinámica de respuesta rápida bajo estrés.

Equilibrio entre Suerte y Habilidad: El azar del dado determina el avance físico por el tablero, pero la retención de conocimientos académicos es el verdadero cuello de botella para ganar el juego.

Bucle de Recuperación (Comeback Loop): Si un jugador avanza muy rápido físicamente pero falla las preguntas, quedará retenido al final de la vuelta, dándole la oportunidad a los jugadores rezagados de alcanzarlo si responden correctamente.

Ritmo de Juego Intermitente: Alternancia de picos de tensión (resolución de preguntas) con momentos de relajación y expectación (lanzamiento de dados y movimiento visual de peones).

3. Estéticas (Aesthetics)

Desafío (Challenge): La satisfacción de demostrar conocimientos académicos para superar las pruebas y desbloquear la siguiente etapa.

Fantasía (Fantasy): Inmersión en una representación humorística y ligera de la vida universitaria (atravesar parciales, participar en Game Jams y superar la entrega de proyectos).

Compañerismo y Competencia (Fellowship & Competition): Experiencia tipo party game en multijugador local, fomentando la rivalidad amistosa en vivo entre compañeros de clase.

Nostalgia e Identidad Visual (Sensation): Estética retro inspirada en la era PS1 (Low-Poly) combinada con la paleta de colores institucional, ofreciendo un entorno 3D colorido, expresivo y con personalidad propia con una base en los colores de la Universidad de Hurlingham.

Link al diagrama UML: https://canva.link/3yhmqlk4sfyd1if  

Declaración de IA generativa: En el proyecto se uso ChatGPT y Gemini

Link al backlog en trello: https://trello.com/b/fH4YGSP3/tablero-prog-de-videojuegos-2

📖 Guía de Usuario — La carrera por el titulo🎯 

Objetivo del Juego: Ser el primer estudiante en completar la carrera universitaria, representada por 3 vueltas completas al tablero (3 años académicos), superando la cuota de exámenes (preguntas de trivia) requerida en cada etapa.

📜 Reglas de Juego 

Lanzamiento y Movimiento:Los jugadores juegan por turnos en modalidad local (1 a 4 jugadores).En tu turno, presiona el botón de Tirar Dado para obtener un número del 1 al 6. Tu peón avanzará automáticamente casilla por casilla.

Casilleros de Evaluación (Trivia):Al caer en una casilla de Pregunta, el juego se pausará y se desplegará una tarjeta de evaluación con 4 opciones y solo 1 correcta.Cuentas con un tiempo límite para responder. Si se agota el tiempo o eliges la opción incorrecta, no sumarás respuestas correctas.

Examen de Fin de Año (Rendimiento Académico):Para poder cruzar la meta de la primera vuelta y avanzar a la Vuelta 2, debes haber acumulado al menos 2 respuestas correctas.

Repetición de Año: Si llegas al final del circuito sin la cuota requerida, no podras avanzar hasta que consigas la cantidad de respuestas correctas requeridas.

Condición de Victoria:El primer jugador en completar con éxito la Vuelta 3 (3er año) obtendrá el título universitario y ganará la partida.

🎮 Mapa de Controles

El juego utiliza una interfaz gráfica interactiva basada en puntero (Mouse)

Clic Izquierdo sobre Tirar Dado Generara un número aleatorio (1-6) e inicia el desplazamiento físico.

Seleccionar respuesta haciendo clic Izquierdo sobre Opción (1 a 4).

Reanuda el turno automáticamente al responder y cierra el panel de evaluación y pasa el turno al siguiente jugador.
