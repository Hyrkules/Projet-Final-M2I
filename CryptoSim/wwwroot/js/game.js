window.initDinoGame = () => {
    const wrapper = document.getElementById('dino-game-wrapper');
    const canvas = document.getElementById('dinoCanvas');
    if (!canvas || !wrapper) return;

    const ctx = canvas.getContext('2d');

    let isPlaying = false;
    let score = 0;
    let canRestart = true
    let animationId;

    // --- NOUVEAU : Création des objets Image ---
    const dinoImage = new Image();
    dinoImage.src = 'images/dino_sprite.png'; // REMPLACEZ ICI LE CHEMIN DU DINO

    const obstacleImage = new Image();
    obstacleImage.src = 'images/obstacle_sprite.png'; // REMPLACEZ ICI LE CHEMIN DE L'OBSTACLE

    // --- Dimensions adaptées à un canvas de 800x300 ---
    // Le nouveau sol est à y=280.
    // Avec un dino de 100px de haut, sa position Y sur le sol est 280 - 100 = 180.
    const groundLevelY = 280; // Nouvelle ligne de sol
    const dinoHeight = 100; // Un peu plus grand pour la zone agrandie
    const dinoWidth = 100; // Carré, comme dans votre code

    let dino = {
        x: 50,
        y: groundLevelY - dinoHeight, // y = 180 (position sur le sol)
        w: dinoWidth,
        h: dinoHeight,
        dy: 0,
        jump: -14, // Puissance du saut augmentée pour la zone plus haute
        gravity: 0.6,
        grounded: true
    };

    // Dimensions des obstacles, aussi adaptées (plus hauts/larges)
    const obstacleHeight = 80;
    const obstacleWidth = 80;
    // Position Y de la caisse pour être sur le sol : groundLevelY - obstacleHeight = 200
    const obstacleGroundY = groundLevelY - obstacleHeight;

    let obstacles = [];

    function draw() {
        if (!isPlaying) return;

        // 1. On efface tout
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // 2. On dessine le nouveau "sol"
        ctx.strokeStyle = "rgba(193, 164, 97, 0.4)"; // Opacité augmentée pour mieux le voir
        ctx.lineWidth = 2; // Sol un peu plus épais
        ctx.beginPath();
        ctx.moveTo(0, groundLevelY); // Nouvelle hauteur de sol (280)
        ctx.lineTo(canvas.width, groundLevelY); // Utilise la largeur du canvas (800)
        ctx.stroke();
        ctx.lineWidth = 1; // Reset

        // 3. Physique du Dino
        if (!dino.grounded) {
            dino.dy += dino.gravity;
            dino.y += dino.dy;
        }

        // Limite du sol (Ajustée à y=180)
        if (dino.y >= groundLevelY - dino.h) {
            dino.y = groundLevelY - dino.h;
            dino.dy = 0;
            dino.grounded = true;
        }

        // 4. Dessin du Dino (AVEC IMAGE et dimensions adaptées)
        // Ajout d'une petite lueur (toujours active sur l'image)
        ctx.shadowBlur = 10;
        ctx.shadowColor = "#d4af37";
        ctx.drawImage(dinoImage, dino.x, dino.y, dino.w, dino.h);

        // 5. Gestion des obstacles
        ctx.shadowBlur = 0; // On retire l'ombre pour les obstacles

        for (let i = obstacles.length - 1; i >= 0; i--) {
            let o = obstacles[i];
            o.x -= 6; // Vitesse de défilement (peut-être augmentée si trop lent)

            // Dessin obstacle (Caisse, AVEC IMAGE, dimensions et position ajustées)
            ctx.drawImage(obstacleImage, o.x, obstacleGroundY, obstacleWidth, obstacleHeight);

            // Collision (Ajustée avec les nouvelles dimensions)
            // C'est ici que votre ancien code avait des valeurs fixes ("15", "110+30").
            // J'ai remplacé par des variables pour plus de clarté.
            const hitboxPadding = 20; // Plus ce chiffre est élevé, plus la hitbox est PETITE

            if (dino.x + hitboxPadding < o.x + obstacleWidth - hitboxPadding &&
                dino.x + dino.w - hitboxPadding > o.x + hitboxPadding &&
                dino.y + hitboxPadding < obstacleGroundY + obstacleHeight - hitboxPadding &&
                dino.y + dino.h - hitboxPadding > obstacleGroundY + hitboxPadding) {
                gameOver();
            }

            // Supprimer si sorti de l'écran (Adapté à la nouvelle largeur)
            if (o.x < -(obstacleWidth + 10)) {
                obstacles.splice(i, 1);
                score++;
            }
        }

        // 6. Apparition aléatoire d'obstacles
        if (Math.random() < 0.015) {
            obstacles.push({ x: canvas.width });
        }

        animationId = requestAnimationFrame(draw);
    }

    function gameOver() {
        isPlaying = false;
        canRestart = false; // BLOQUE le redémarrage immédiat
        cancelAnimationFrame(animationId);
        wrapper.classList.remove('game-active');

        // Mise à jour du message
        const messageElement = document.getElementById('game-message');
        if (messageElement) {
            messageElement.innerHTML = `VOUS AVEZ GAGNÉ ${score * 1 } PIKACOIN ! <br> <span style="font-size:0.8em; color:gray;">Attendez un instant... Pikachu se repose</span>`;
        }

        // --- NOUVEAU : Débloquer après 1.5 seconde ---
        setTimeout(() => {
            canRestart = true;
            if (messageElement) {
                messageElement.innerHTML = `<span style="font-size:0.8em; color:gray;"> PIKA EST PRÊT !</span>`;
            }
        }, 4000); // 1500ms = 1.5s

        // Reset
        obstacles = [];
        score = 0;
        dino.y = 180;
    }



    // Gestion du clavier sur le WRAPPER
    wrapper.addEventListener('keydown', (e) => {
        if (e.code === 'Space') {
            e.preventDefault();

            if (!isPlaying) {
                // On ne lance le jeu QUE SI canRestart est vrai
                if (canRestart) {
                    isPlaying = true;
                    wrapper.classList.add('game-active');
                    draw();
                }
                return; // On sort pour ne pas faire sauter le dino au premier clic
            }

            // Saut normal pendant le jeu
            if (dino.grounded) {
                dino.dy = dino.jump;
                dino.grounded = false;
            }
        }
    });
};