pipeline {
    agent any
    environment {
        NUGET_KEY = credentials('jenkins-nuget-key')
    }
    stages {
        stage('Install nuke') {
            steps {
                withDotNet(sdk: '7.0') {
                    sh 'dotnet tool install --global Nuke.GlobalTool | echo "already installed"'
                }
            }
        }
        stage('Nuke Pack') {
            steps {
                withDotNet(sdk: '7.0') {
                    sh '/var/jenkins_home/.dotnet/tools/nuke pack'
                }
            }
        }
    }
}