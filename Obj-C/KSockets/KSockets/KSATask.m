//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSATask.h"

@interface KSATask ()

@property NSOperationQueue * queue;

@end



@implementation KSATask

- (id)init
{
    self = [super init];

    self.queue = [[NSOperationQueue alloc] init];
    [self.queue setMaxConcurrentOperationCount:1];
    
    return self;
}

- (void)run
{
    [self.queue addOperation:self];
}

- (void)stop
{
    [self.queue cancelAllOperations];
}

@end
