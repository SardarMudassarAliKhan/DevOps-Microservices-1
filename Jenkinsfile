pipeline {
    agent any
    
    options {
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '5'))
    }   
    
    stages {
        stage('Checkout Source') {
            steps {
                checkout scm
            }
        }

        stage('Build') {
            steps {
                script {
                    echo 'Cleaning up old production images...'
                    sh 'docker rmi microservice-1:latest || true'

                    echo 'Building the new Docker image...'
                    sh 'docker build --no-cache -t microservice-1:latest -f DevOps-Microservices-2/Dockerfile .'
                }
            }
        }

        stage('Push and Deploy') {
            steps {
                script {
                    echo 'Ensuring isolated persistent data protection keys and uploads directories exist on host...'
                    sh 'mkdir -p /home/ubuntu/microservice-1/aspnet-keys'
                    sh 'mkdir -p /home/ubuntu/microservice-1/app-uploads/logos'
                    sh 'chmod -R 777 /home/ubuntu/microservice-1/app-uploads'
                    
                    echo 'Stopping old active container if it exists...'
                    sh 'docker stop microservice-1-web || true'
                    sh 'docker rm microservice-1-web || true'
                    
                    echo 'Checking availability of Loki logging driver...'
                    def lokiAvailable = sh(script: "docker plugin ls --format '{{.Name}}' | grep -q 'loki'", returnStatus: true) == 0
                    
                    def loggingOpts = lokiAvailable ? 
                        '--log-driver=loki --log-opt loki-url="http://130.131.46.90:3100/loki/api/v1/push" --log-opt loki-external-labels="container_name={{.Name}}"' : 
                        '--log-driver=json-file --log-opt max-size=10m --log-opt max-file=3'

                    echo "Running new container with logging strategy: ${lokiAvailable ? 'Loki' : 'JSON File Fallback'}..."
                    
                    // FIXED: 
                    // 1. Unique container name (microservice-1-web)
                    // 2. Isolated host paths (/home/ubuntu/microservice-1/...) to avoid overlapping with microservice-2
                    // 3. Correct non-root home directory path (/home/app/.aspnet/DataProtection-Keys) matching USER $APP_UID
                    // 4. Exposed on unique host port (8082)
                    sh """
                        docker run -d --restart always --name microservice-1-web \
                        ${loggingOpts} \
                        --env "ASPNETCORE_ENVIRONMENT=Production" \
                        --env "ASPNETCORE_URLS=http://0.0.0.0:8080" \
                        -v /home/ubuntu/microservice-1/aspnet-keys:/home/app/.aspnet/DataProtection-Keys \
                        -v /home/ubuntu/microservice-1/app-uploads:/app/wwwroot/uploads \
                        --network mudassar -p 8082:8080 microservice-1:latest
                    """
                }
            }
        }

        stage('Cleanup') {
            steps {
                script {
                    echo 'Cleaning up dangling images...'
                    sh 'docker image prune -f'
                }
            }
        }
    }
}