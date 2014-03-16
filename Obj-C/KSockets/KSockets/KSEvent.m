//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSEvent.h"

@interface KSEventListener : NSObject

@property id    instance;
@property SEL   selector;

- (id)initWith:(id)instance selector:(SEL)selector;
- (Boolean)isEqualsWith:(id)instance selector:(SEL)selector;
- (void)dispatch:(id)object waitUntilDone:(BOOL)waitUntilDone;

@end

@implementation KSEventListener

- (id)initWith:(id)instance selector:(SEL)selector
{
    self = [super init];
    self.instance = instance;
    self.selector = selector;
    return self;
}

- (Boolean)isEqualsWith:(id)instance selector:(SEL)selector
{
    if(self.instance != instance) return NO;
    if(self.selector != selector) return NO;
    return YES;
}

- (void)dispatch:(id)object waitUntilDone:(BOOL)waitUntilDone
{
    // 正常なコードなのにワーニングが出るので、#pragmaで無視
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    [self.instance performSelectorOnMainThread:self.selector withObject:object waitUntilDone:waitUntilDone];
#pragma clang diagnostic pop
}

@end



@interface KSEvent ()

@property NSMutableArray * listeners;

@end

@implementation KSEvent

-(id)init
{
    self = [super init];
    self.listeners = [NSMutableArray array];
    return self;
}

- (void)addEventListener:(id)instance selector:(SEL)selector;
{
    KSEventListener * listener = [[KSEventListener alloc] initWith:instance selector:selector];
    [self.listeners addObject:listener];
}

- (void)removeEventListener:(id)instance selector:(SEL)selector
{
    NSMutableArray * removeObjects = [NSMutableArray array];
    for(KSEventListener * listener in self.listeners)
    {
        if([listener isEqualsWith:instance selector:selector])
        {
            [removeObjects addObject:listener];
        }
    }
    
    [self.listeners removeObjectsInArray:removeObjects];
}


- (void)dispatchEventWithObject:(id)object waitUntilDone:(BOOL)waitUntilDone
{
    for(KSEventListener * listener in self.listeners)
    {
        [listener dispatch:object waitUntilDone:waitUntilDone];
    }
}


@end
