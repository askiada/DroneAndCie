#!/usr/bin/env python
import sys
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.gridspec as gridspec


def buildPlot(path, fileName1, suptitle, xlabel, ylabel, fileName2, xlabel2, ylabel2, fileName3):
    gs = gridspec.GridSpec(3*10,5)
    plt.figure(figsize=(15, 17))
    plt.suptitle(suptitle, fontsize=16)
    colors = ['blue', 'red', 'black', 'brown', 'green', 'darkorchid', 'salmon', 'teal','deepskyblue', 'silver']
    
    dataGenetic = np.genfromtxt(path + fileName1+'.csv', delimiter=';')
    
    best = dataGenetic[:,1]
    mean = dataGenetic[:,2]
    std = dataGenetic[:,3]
    median = dataGenetic[:,4]
    
    
    plt.subplot(gs[0:(0) + 9,:])
    
    plt.fill_between(np.arange(mean.shape[0]), mean.flatten(), (mean.flatten() + std.flatten()), color=colors[0], alpha=0.3,label = "Standard Deviation")
    plt.fill_between(np.arange(mean.shape[0]), mean.flatten(), (mean.flatten() - std.flatten()), color=colors[0], alpha=0.3)
    plt.plot(mean, color=colors[0], label = "Mean")
    plt.plot(best, color=colors[1], label = "Best")
    plt.plot(median, color=colors[4], ls=':', label = "Median")
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.legend()
    
    
    dataGenSession = np.genfromtxt(path + fileName2+'.csv', delimiter=';')
    
    angleRandomRotation = dataGenSession[:,1]
    wind = dataGenSession[:,2]
    #std = dataGenSession[:,3]
    #median = dataGenSession[:,4]
    
    plt.subplot(gs[10:(10) + 9,:])
    
    #plt.fill_between(np.arange(mean.shape[0]), mean.flatten(), (mean.flatten() + std.flatten()), color=colors[0], alpha=0.3,label = "Standard Deviation")
    #plt.fill_between(np.arange(mean.shape[0]), mean.flatten(), (mean.flatten() - std.flatten()), color=colors[0], alpha=0.3)
    plt.plot(angleRandomRotation, color=colors[0], label = "Angle Random Rotation")
    plt.plot(wind, color=colors[1], label = "Wind")
    #plt.plot(median, color=colors[4], ls=':', label = "Wind")
    plt.xlabel(xlabel2)
    plt.ylabel(ylabel2)
    plt.legend()
    
    
    #print(gs[20,1])
    dataParams = np.genfromtxt(path + fileName3+'.csv', delimiter=';', dtype=None)
    plt.subplot(gs[20:(20) + 9,:])
    for i in range(dataParams.shape[0]):
        plt.text(0.1,0.1*i +0.05, dataParams[dataParams.shape[0] - 1 - i].decode("utf-8"))
    
    
    
    
    plt.subplots_adjust(bottom=0.05, left=.05, right=.99, top=.95)
        
    # print(savepath + '/'+namePlot+'.' + format)
    plt.savefig(path + '/Session.' + "pdf", format="pdf")
    
    plt.close()

def main():
    if len(sys.argv) > 8:
        #print(sys.argv)
        buildPlot(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5],sys.argv[6], sys.argv[7], sys.argv[8], sys.argv[9])
    
    
if __name__ == '__main__':main()
