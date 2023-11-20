pipeline {
    agent any
    environment {
        NUGET_KEY = credentials('jenkins-nuget-key')
        TAG_NAME = sh(returnStdout: true, script: "git describe --tags").trim()
    }
    stages {
        stage('Install nuke') {
            steps {
                withDotNet(sdk: '8.0') {
                    sh 'dotnet tool install --global Nuke.GlobalTool | echo "already installed"'
                }
            }
        }
        stage('Nuke Pack') {
            steps {
                withDotNet(sdk: '7.0') {
                    withDotNet(sdk: '8.0') {
                        sh '/var/jenkins_home/.dotnet/tools/nuke pack'
                    }
                }
            }
        }
        stage('Nuke Push if tagged') {
            when {
                buildingTag()
            }
            steps {
                withDotNet(sdk: '7.0') {
                    withDotNet(sdk: '8.0') {
                        sh '/var/jenkins_home/.dotnet/tools/nuke push --NugetApiKey "$NUGET_KEY"'
                    }
                }
            }
        }
    }
}