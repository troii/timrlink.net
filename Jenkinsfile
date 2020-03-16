#!/usr/bin/env groovy

properties([
    parameters([
        booleanParam(name: "BUILD_ARTIFACTS", description: "Build and archive artifacts", defaultValue: false)
    ]),
    buildDiscarder(logRotator(artifactDaysToKeepStr: '', artifactNumToKeepStr: '', daysToKeepStr: '', numToKeepStr: '10'))
])

def slack(String color, String message, List channels = []) {
    for (channel in [ "#builds", "#timr-status" ] + channels) {
        slackSend(channel: channel, color: color, message: message)
    }
}

node(label: "docker") {
    try {
    
        stage("Prepare") {
            checkout scm
            def commitHash = sh(script: "git rev-parse --short HEAD", returnStdout: true).trim()
        }
        
        stage("Build") {
            sh "docker run -v \$(pwd):/mnt/timrlink mcr.microsoft.com/dotnet/core/sdk:3.1 dotnet build /mnt/timrlink -c Release"
        }
        
        stage("Test") {
            if (currentBuild.result == null) {
                warnError(message: "This commit has test failures.") {
                    sh "docker run -v \$(pwd):/mnt/timrlink mcr.microsoft.com/dotnet/core/sdk:3.1 dotnet test /mnt/timrlink -c Release"
                }
                // junit allowEmptyResults: true, testResults: "**/build/test-results/**/*.xml"
            }
        }

        stage("Publish") {
            if (currentBuild.result == null) {
                warnError(message: "Publish failed.") {
                    sh "docker run -v \$(pwd):/mnt/timrlink mcr.microsoft.com/dotnet/core/sdk:3.1 dotnet publish /mnt/timrlink/timrlink.net.CLI -c Release -r win7-x64 --self-contained"
                    zip(zipFile: "timrlink-win7-x64.zip", dir: "./timrlink.net.CLI/bin/Release/netcoreapp2.0/win7-x64/publish", archive: true)

                    sh "docker run -v \$(pwd):/mnt/timrlink mcr.microsoft.com/dotnet/core/sdk:3.1 dotnet publish /mnt/timrlink/timrlink.net.CLI -c Release -r osx-x64 --self-contained"
                    zip(zipFile: "timrlink-osx-x64.zip", dir: "./timrlink.net.CLI/bin/Release/netcoreapp2.0/osx-x64/publish", archive: true)

                    sh "docker run -v \$(pwd):/mnt/timrlink mcr.microsoft.com/dotnet/core/sdk:3.1 dotnet publish /mnt/timrlink/timrlink.net.CLI -c Release -r ubuntu.18.04-x64 --self-contained"
                    zip(zipFile: "timrlink-ubuntu.18.04-x64.zip", dir: "./timrlink.net.CLI/bin/Release/netcoreapp2.0/ubuntu.18.04-x64/publish", archive: true)

                    slack("good", "Docker Image $IMAGE_NAME build for ${currentBuild.fullDisplayName} finished", ["#timr-chat"])
                }
            }
        }

    } finally {
        if (currentBuild.result != null) {
            slack("danger", "The pipeline ${currentBuild.fullDisplayName} failed. (<${env.BUILD_URL}|Open>)")
        }
    }
}
